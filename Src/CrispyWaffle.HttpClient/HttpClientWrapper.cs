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
            // Merge defaults and per-request options first.
            var effectiveOptions = MergeOptions(_defaults, perRequest);

            // Use serializer from merged options, fallback to default.
            var serializer = effectiveOptions.Serializer ?? _defaultSerializer;

            // Create HttpClient using the effective options (so named client, base address, timeout, etc. apply).
            var client = CreateClient(effectiveOptions);

            var sw = Stopwatch.StartNew();

            // The action will create a NEW HttpRequestMessage each time it's invoked (important for retries).
            Func<CancellationToken, Task<HttpResponseMessage>> action = async ct =>
            {
                // Create a new request per attempt - ensures headers/content are fresh for retries.
                using var req = new HttpRequestMessage(method, url);

                if (body != null)
                {
                    // Serialize the body (synchronous serializer expected here).
                    string payload = serializer.Serialize(body);
                    req.Content = new StringContent(payload, Encoding.UTF8, effectiveOptions.UseJsonContentType ? "application/json" : "text/plain");
                }

                // Apply per-request (merged) headers to the request message.
                if (effectiveOptions.DefaultRequestHeaders != null)
                {
                    foreach (var kv in effectiveOptions.DefaultRequestHeaders)
                    {
                        if (!req.Headers.Contains(kv.Key))
                            req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                    }
                }

                // Send with ResponseHeadersRead to start streaming the body (we will read it explicitly afterward).
                // This avoids buffering the entire response in some handlers.
                return await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false);
            };

            // Execute the action with retry policy. The policy is expected to return the final response (or null + exception).
            var (resp, ex) = await RetryPolicy.ExecuteAsync(
                action,
                effectiveOptions.RetryCount,
                effectiveOptions.RetryBaseDelayMs,
                effectiveOptions.MaxRetryDelayMs,
                cancellationToken).ConfigureAwait(false);

            sw.Stop();

            // If no response after retries
            if (resp == null)
            {
                if (effectiveOptions.ThrowOnError)
                {
                    throw new HttpClientException("HTTP request failed after retries", null, null, ex);
                }

                var errors = new List<string> { ex?.Message ?? "Unknown error" };
                return HttpResponse<T>.Failure(null, null, errors, null, sw.Elapsed);
            }

            // IMPORTANT: dispose HttpResponseMessage after we've finished reading headers/body.
            using (resp)
            {
                string? raw = null;
                try
                {
                    // If there is content, read it as string. Keep it simple and synchronous from serializer's perspective.
                    if (resp.Content != null)
                    {
                        raw = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception readEx)
                {
                    // Failed to read content.
                    _logger?.LogError(readEx, "Failed to read response content for {Method} {Url}", method, url);
                    if (effectiveOptions.ThrowOnError)
                    {
                        throw new HttpClientException("Failed to read response content", resp.StatusCode, null, readEx);
                    }

                    var readErrors = new List<string> { readEx.Message };
                    return HttpResponse<T>.Failure(resp.StatusCode, null, readErrors, resp.Headers.ToDictionary(h => h.Key, h => h.Value.AsEnumerable()), sw.Elapsed);
                }

                // Gather headers (including content headers).
                var headers = resp.Headers.ToDictionary(h => h.Key, h => h.Value.AsEnumerable());
                if (resp.Content?.Headers != null)
                {
                    foreach (var h in resp.Content.Headers)
                        headers[h.Key] = h.Value.AsEnumerable();
                }

                // Successful HTTP status
                if (resp.IsSuccessStatusCode)
                {
                    T? data = default;
                    try
                    {
                        // If raw body is present, try to deserialize; otherwise default(T).
                        if (!string.IsNullOrWhiteSpace(raw))
                        {
                            data = serializer.Deserialize<T>(raw);
                        }
                    }
                    catch (Exception deserEx)
                    {
                        // Deserialization failed - log and either throw or return failure depending on options.
                        _logger?.LogError(deserEx, "Failed to deserialize response body to {Type} for {Method} {Url}", typeof(T).FullName, method, url);
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
                    // Non-success HTTP status
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