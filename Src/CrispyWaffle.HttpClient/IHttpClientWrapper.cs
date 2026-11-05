using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CrispyWaffle.HttpClient;

/// <summary>
/// High level HTTP client wrapper - primarily async methods (sync wrappers provided but not recommended).
/// </summary>
public interface IHttpClientWrapper
{
    /// <summary>
    /// Sends a GET request and deserializes the response body into <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type the response body is deserialized into.</typeparam>
    /// <param name="url">The request URL, absolute or relative to the configured base address.</param>
    /// <param name="options">
    /// The options for this request; <c>null</c> to use the registered defaults. A non-null value replaces
    /// some of those defaults even for properties it leaves unset, such as
    /// <see cref="HttpRequestOptions.ThrowOnError"/>; see the remarks on <see cref="HttpRequestOptions"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the request. It is observed until the response headers arrive and during the waits
    /// between retries, but not while the response body is read.
    /// </param>
    /// <returns>A task whose result describes the outcome of the request.</returns>
    Task<HttpResponse<T>> GetAsync<T>(
        string url,
        HttpRequestOptions? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sends a POST request with the serialized <paramref name="body"/> and deserializes the response body
    /// into <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body.</typeparam>
    /// <typeparam name="TResponse">The type the response body is deserialized into.</typeparam>
    /// <param name="url">The request URL, absolute or relative to the configured base address.</param>
    /// <param name="body">The request body; no content is sent when it is <c>null</c>.</param>
    /// <param name="options">
    /// The options for this request; <c>null</c> to use the registered defaults. A non-null value replaces
    /// some of those defaults even for properties it leaves unset, such as
    /// <see cref="HttpRequestOptions.ThrowOnError"/>; see the remarks on <see cref="HttpRequestOptions"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the request. It is observed until the response headers arrive and during the waits
    /// between retries, but not while the response body is read.
    /// </param>
    /// <returns>A task whose result describes the outcome of the request.</returns>
    Task<HttpResponse<TResponse>> PostAsync<TRequest, TResponse>(
        string url,
        TRequest? body = default,
        HttpRequestOptions? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sends a PUT request with the serialized <paramref name="body"/> and deserializes the response body
    /// into <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body.</typeparam>
    /// <typeparam name="TResponse">The type the response body is deserialized into.</typeparam>
    /// <param name="url">The request URL, absolute or relative to the configured base address.</param>
    /// <param name="body">The request body; no content is sent when it is <c>null</c>.</param>
    /// <param name="options">
    /// The options for this request; <c>null</c> to use the registered defaults. A non-null value replaces
    /// some of those defaults even for properties it leaves unset, such as
    /// <see cref="HttpRequestOptions.ThrowOnError"/>; see the remarks on <see cref="HttpRequestOptions"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the request. It is observed until the response headers arrive and during the waits
    /// between retries, but not while the response body is read.
    /// </param>
    /// <returns>A task whose result describes the outcome of the request.</returns>
    Task<HttpResponse<TResponse>> PutAsync<TRequest, TResponse>(
        string url,
        TRequest? body = default,
        HttpRequestOptions? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sends a DELETE request. A response body, if any, is deserialized as an <see cref="object"/>.
    /// </summary>
    /// <param name="url">The request URL, absolute or relative to the configured base address.</param>
    /// <param name="options">
    /// The options for this request; <c>null</c> to use the registered defaults. A non-null value replaces
    /// some of those defaults even for properties it leaves unset, such as
    /// <see cref="HttpRequestOptions.ThrowOnError"/>; see the remarks on <see cref="HttpRequestOptions"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the request. It is observed until the response headers arrive and during the waits
    /// between retries, but not while the response body is read.
    /// </param>
    /// <returns>A task whose result describes the outcome of the request.</returns>
    Task<HttpResponse<object?>> DeleteAsync(
        string url,
        HttpRequestOptions? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sends a GET request synchronously and deserializes the response body into <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type the response body is deserialized into.</typeparam>
    /// <param name="url">The request URL, absolute or relative to the configured base address.</param>
    /// <param name="options">
    /// The options for this request; <c>null</c> to use the registered defaults. A non-null value replaces
    /// some of those defaults even for properties it leaves unset, such as
    /// <see cref="HttpRequestOptions.ThrowOnError"/>; see the remarks on <see cref="HttpRequestOptions"/>.
    /// </param>
    /// <returns>The outcome of the request.</returns>
    /// <remarks>
    /// This method blocks on <see cref="GetAsync{T}"/> and may deadlock when called under a synchronization context.
    /// Use it with caution and prefer the asynchronous method.
    /// </remarks>
    HttpResponse<T> Get<T>(string url, HttpRequestOptions? options = null);

    /// <summary>
    /// Sends a POST request synchronously with the serialized <paramref name="body"/> and deserializes the response body
    /// into <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body.</typeparam>
    /// <typeparam name="TResponse">The type the response body is deserialized into.</typeparam>
    /// <param name="url">The request URL, absolute or relative to the configured base address.</param>
    /// <param name="body">The request body; no content is sent when it is <c>null</c>.</param>
    /// <param name="options">
    /// The options for this request; <c>null</c> to use the registered defaults. A non-null value replaces
    /// some of those defaults even for properties it leaves unset, such as
    /// <see cref="HttpRequestOptions.ThrowOnError"/>; see the remarks on <see cref="HttpRequestOptions"/>.
    /// </param>
    /// <returns>The outcome of the request.</returns>
    /// <remarks>
    /// This method blocks on <see cref="PostAsync{TRequest, TResponse}"/> and may deadlock when called under a synchronization context.
    /// Use it with caution and prefer the asynchronous method.
    /// </remarks>
    HttpResponse<TResponse> Post<TRequest, TResponse>(
        string url,
        TRequest? body = default,
        HttpRequestOptions? options = null
    );

    /// <summary>
    /// Sends a PUT request synchronously with the serialized <paramref name="body"/> and deserializes the response body
    /// into <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request body.</typeparam>
    /// <typeparam name="TResponse">The type the response body is deserialized into.</typeparam>
    /// <param name="url">The request URL, absolute or relative to the configured base address.</param>
    /// <param name="body">The request body; no content is sent when it is <c>null</c>.</param>
    /// <param name="options">
    /// The options for this request; <c>null</c> to use the registered defaults. A non-null value replaces
    /// some of those defaults even for properties it leaves unset, such as
    /// <see cref="HttpRequestOptions.ThrowOnError"/>; see the remarks on <see cref="HttpRequestOptions"/>.
    /// </param>
    /// <returns>The outcome of the request.</returns>
    /// <remarks>
    /// This method blocks on <see cref="PutAsync{TRequest, TResponse}"/> and may deadlock when called under a synchronization context.
    /// Use it with caution and prefer the asynchronous method.
    /// </remarks>
    HttpResponse<TResponse> Put<TRequest, TResponse>(
        string url,
        TRequest? body = default,
        HttpRequestOptions? options = null
    );

    /// <summary>
    /// Sends a DELETE request synchronously. A response body, if any, is deserialized as an <see cref="object"/>.
    /// </summary>
    /// <param name="url">The request URL, absolute or relative to the configured base address.</param>
    /// <param name="options">
    /// The options for this request; <c>null</c> to use the registered defaults. A non-null value replaces
    /// some of those defaults even for properties it leaves unset, such as
    /// <see cref="HttpRequestOptions.ThrowOnError"/>; see the remarks on <see cref="HttpRequestOptions"/>.
    /// </param>
    /// <returns>The outcome of the request.</returns>
    /// <remarks>
    /// This method blocks on <see cref="DeleteAsync"/> and may deadlock when called under a synchronization context.
    /// Use it with caution and prefer the asynchronous method.
    /// </remarks>
    HttpResponse<object?> Delete(string url, HttpRequestOptions? options = null);
}
