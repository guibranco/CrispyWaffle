using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CrispyWaffle.HttpClient
{
    /// <summary>
    /// High level HTTP client wrapper - primarily async methods (sync wrappers provided but not recommended).
    /// </summary>
    public interface IHttpClientWrapper
    {
        Task<HttpResponse<T>> GetAsync<T>(
            string url,
            HttpRequestOptions? options = null,
            CancellationToken cancellationToken = default
        );
        Task<HttpResponse<TResponse>> PostAsync<TRequest, TResponse>(
            string url,
            TRequest? body = default,
            HttpRequestOptions? options = null,
            CancellationToken cancellationToken = default
        );
        Task<HttpResponse<TResponse>> PutAsync<TRequest, TResponse>(
            string url,
            TRequest? body = default,
            HttpRequestOptions? options = null,
            CancellationToken cancellationToken = default
        );
        Task<HttpResponse<object?>> DeleteAsync(
            string url,
            HttpRequestOptions? options = null,
            CancellationToken cancellationToken = default
        );

        // sync helpers (block on async) - use with caution (may deadlock in sync contexts)
        HttpResponse<T> Get<T>(string url, HttpRequestOptions? options = null);
        HttpResponse<TResponse> Post<TRequest, TResponse>(
            string url,
            TRequest? body = default,
            HttpRequestOptions? options = null
        );
        HttpResponse<TResponse> Put<TRequest, TResponse>(
            string url,
            TRequest? body = default,
            HttpRequestOptions? options = null
        );
        HttpResponse<object?> Delete(string url, HttpRequestOptions? options = null);
    }
}
