using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.HttpClient.Exceptions;
using CrispyWaffle.HttpClient.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CrispyWaffle.HttpClient
{
    /// <summary>
    /// Robust HttpClient wrapper with retry/backoff and pluggable serializer.
    /// </summary>
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpRequestOptions _defaults;
        private readonly IJsonSerializer _defaultSerializer;
        private readonly ILogger<HttpClientWrapper>? _logger;

        public HttpClientWrapper(IHttpClientFactory httpClientFactory, IOptions<HttpRequestOptions> options, IJsonSerializer? serializer = null, ILogger<HttpClientWrapper>? logger = null)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _defaults = options?.Value ?? new HttpRequestOptions();
            _defaultSerializer = serializer ?? new SystemTextJsonSerializer();
            _logger = logger;
        }

        private System.Net.Http.HttpClient CreateClient(HttpRequestOptions? perRequest)
        {
            var name = perRequest?.NamedHttpClient ?? _defaults.NamedHttpClient;
            System.Net.Http.HttpClient client = string.IsNullOrEmpty(name)
                ? _httpClientFactory.CreateClient()
                : _httpClientFactory.CreateClient(name);

            var baseAddr = perRequest?.BaseAddress ?? _defaults.BaseAddress;
            if (!string.IsNullOrEmpty(baseAddr) && client.BaseAddress == null)
            {
                client.BaseAddress = new Uri(baseAddr);
            }

            var timeoutSeconds = perRequest?.TimeoutSeconds ?? _defaults.TimeoutSeconds;
            client.Timeout = TimeSpan.FromSeconds(Math.Max(1, timeoutSeconds));

            // Merge default headers (global) and per-request headers
            var headers = new Dictionary<string, string>();
            if (_defaults.DefaultRequestHeaders != null)
            {
                foreach (var kv in _defaults.DefaultRequestHeaders)
                    headers[kv.Key] = kv.Value;
            }
            if (perRequest?.DefaultRequestHeaders != null)
            {
                foreach (var kv in perRequest.DefaultRequestHeaders)
                    headers[kv.Key] = kv.Value;
            }

            // Set headers (be careful to not duplicate)
            foreach (var kv in headers)
            {
                if (!client.DefaultRequestHeaders.Contains(kv.Key))
                {
                    try
                    {
                        client.DefaultRequestHeaders.Add(kv.Key, kv.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Failed to set default header {Header}", kv.Key);
                    }
                }
            }

            // Ensure Accept header
            if (client.DefaultRequestHeaders.Accept.Count == 0)
            {
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            }

            return client;
        }

        #region Public Async Methods

        public Task<HttpResponse<T>> GetAsync<T>(string url, HttpRequestOptions? options = null, CancellationToken cancellationToken = default)
            => SendAsync<T>(HttpMethod.Get, url, null, options, cancellationToken);

        public Task<HttpResponse<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest? body = default, HttpRequestOptions? options = null, CancellationToken cancellationToken = default)
            => SendAsync<TResponse>(HttpMethod.Post, url, body, options, cancellationToken);

        public Task<HttpResponse<TResponse>> PutAsync<TRequest, TResponse>(string url, TRequest? body = default, HttpRequestOptions? options = null, CancellationToken cancellationToken = default)
            => SendAsync<TResponse>(HttpMethod.Put, url, body, options, cancellationToken);

        public Task<HttpResponse<object?>> DeleteAsync(string url, HttpRequestOptions? options = null, CancellationToken cancellationToken = default)
            => SendAsync<object?>(HttpMethod.Delete, url, null, options, cancellationToken);

        #endregion

        #region Public Sync wrappers (use with caution)

        public HttpResponse<T> Get<T>(string url, HttpRequestOptions? options = null)
            => GetAsync<T>(url, options).GetAwaiter().GetResult();

        public HttpResponse<TResponse> Post<TRequest, TResponse>(string url, TRequest? body = default, HttpRequestOptions? options = null)
            => PostAsync<TRequest, TResponse>(url, body, options).GetAwaiter().GetResult();

        public HttpResponse<TResponse> Put<TRequest, TResponse>(string url, TRequest? body = default, HttpRequestOptions? options = null)
            => PutAsync<TRequest, TResponse>(url, body, options).GetAwaiter().GetResult();

        public HttpResponse<object?> Delete(string url, HttpRequestOptions? options = null)
            => DeleteAsync(url, options).GetAwaiter().GetResult();

        #endregion

        #region Core Send

        private async Task<HttpResponse<T>> SendAsync<T>(HttpMethod method, string url, object? body, HttpRequestOptions? perRequest, CancellationToken cancellationToken)
        {
            var effectiveOptions = MergeOptions(_defaults, perRequest);
            var serializer = perRequest?.Serializer ?? _defaultSerializer;
            var client = CreateClient(perRequest);
            var sw = Stopwatch.StartNew();

            Func<CancellationToken, Task<HttpResponseMessage>> action = async ct =>
            {
                using var req = new HttpRequestMessage(method, url);

                if (body != null)
                {
                    string payload = serializer.Serialize(body);
                    req.Content = new StringContent(payload, Encoding.UTF8, effectiveOptions.UseJsonContentType ? "application/json" : "text/plain");
                }

                // Apply per-request headers (non-default)
                if (perRequest?.DefaultRequestHeaders != null)
                {
                    foreach (var kv in perRequest.DefaultRequestHeaders)
                    {
                        if (!req.Headers.Contains(kv.Key))
                            req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                    }
                }

                // send
                var response = await client.SendAsync(req, HttpCompletionOption.ResponseContentRead, ct).ConfigureAwait(false);
                return response;
            };

            var (resp, ex) = await RetryPolicy.ExecuteAsync(
                action,
                effectiveOptions.RetryCount,
                effectiveOptions.RetryBaseDelayMs,
                effectiveOptions.MaxRetryDelayMs,
                cancellationToken).ConfigureAwait(false);

            sw.Stop();

            if (resp != null)
            {
                var raw = resp.Content != null ? await resp.Content.ReadAsStringAsync().ConfigureAwait(false) : null;
                var headers = resp.Headers.ToDictionary(h => h.Key, h => h.Value.AsEnumerable());
                // include content headers too
                if (resp.Content?.Headers != null)
                {
                    foreach (var h in resp.Content.Headers)
                        headers[h.Key] = h.Value.AsEnumerable();
                }

                if (resp.IsSuccessStatusCode)
                {
                    // Try deserialize; if empty raw -> default(T)
                    T? data = default;
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(raw))
                        {
                            data = serializer.Deserialize<T>(raw);
                        }
                    }
                    catch (Exception deserEx)
                    {
                        _logger?.LogError(deserEx, "Failed to deserialize response body to {Type}", typeof(T).FullName);
                        // We treat deserialization failure as an error
                        if (effectiveOptions.ThrowOnError)
                        {
                            throw new HttpClientException($"Failed to deserialize response to {typeof(T)}", resp.StatusCode, raw, deserEx);
                        }

                        return HttpResponse<T>.Failure(resp.StatusCode, raw, new List<string> { "DeserializationFailed: " + deserEx.Message }, headers, sw.Elapsed);
                    }

                    return HttpResponse<T>.Success(data, resp.StatusCode, raw, headers, sw.Elapsed);
                }
                else
                {
                    var errors = new List<string> { $"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}" };
                    if (!string.IsNullOrWhiteSpace(raw))
                        errors.Add(raw);

                    if (effectiveOptions.ThrowOnError)
                    {
                        throw new HttpClientException($"HTTP request failed with {(int)resp.StatusCode}", resp.StatusCode, raw);
                    }

                    return HttpResponse<T>.Failure(resp.StatusCode, raw, errors, headers, sw.Elapsed);
                }
            }
            else
            {
                // No response (exception)
                if (effectiveOptions.ThrowOnError)
                {
                    throw new HttpClientException("HTTP request failed after retries", null, null, ex);
                }

                var errors = new List<string> { ex?.Message ?? "Unknown error" };
                return HttpResponse<T>.Failure(null, null, errors, null, sw.Elapsed);
            }
        }

        #endregion

        #region Helpers

        private static HttpRequestOptions MergeOptions(HttpRequestOptions defaults, HttpRequestOptions? perRequest)
        {
            if (perRequest == null) return defaults;
            // create new instance merging properties - only simple properties merged here
            return new HttpRequestOptions
            {
                BaseAddress = perRequest.BaseAddress ?? defaults.BaseAddress,
                TimeoutSeconds = perRequest.TimeoutSeconds != default ? perRequest.TimeoutSeconds : defaults.TimeoutSeconds,
                RetryCount = perRequest.RetryCount != default ? perRequest.RetryCount : defaults.RetryCount,
                RetryBaseDelayMs = perRequest.RetryBaseDelayMs != default ? perRequest.RetryBaseDelayMs : defaults.RetryBaseDelayMs,
                MaxRetryDelayMs = perRequest.MaxRetryDelayMs != default ? perRequest.MaxRetryDelayMs : defaults.MaxRetryDelayMs,
                ThrowOnError = perRequest.ThrowOnError,
                DefaultRequestHeaders = MergeHeaders(defaults.DefaultRequestHeaders, perRequest.DefaultRequestHeaders),
                NamedHttpClient = perRequest.NamedHttpClient ?? defaults.NamedHttpClient,
                UseJsonContentType = perRequest.UseJsonContentType,
                Serializer = perRequest.Serializer ?? defaults.Serializer
            };
        }

        private static IDictionary<string, string>? MergeHeaders(IDictionary<string, string>? a, IDictionary<string, string>? b)
        {
            if (a == null && b == null) return null;
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (a != null)
            {
                foreach (var kv in a) dict[kv.Key] = kv.Value;
            }
            if (b != null)
            {
                foreach (var kv in b) dict[kv.Key] = kv.Value;
            }
            return dict;
        }

        #endregion
    }
}
