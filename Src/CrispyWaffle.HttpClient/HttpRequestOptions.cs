using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text.Json;
using CrispyWaffle.HttpClient.Serialization;

namespace CrispyWaffle.HttpClient;

/// <summary>
/// Options used as defaults (via IOptions) and can be overriden per-request.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="HttpClientWrapper"/> combines the instance passed to a request with the registered
/// defaults one property at a time. Not every default is inherited.
/// </para>
/// <list type="bullet">
/// <item><description>
/// <see cref="BaseAddress"/>, <see cref="NamedHttpClient"/> and <see cref="Serializer"/> take the
/// per-request value when it is not <c>null</c>, otherwise the default.
/// </description></item>
/// <item><description>
/// <see cref="TimeoutSeconds"/>, <see cref="RetryCount"/>, <see cref="RetryBaseDelayMs"/> and
/// <see cref="MaxRetryDelayMs"/> take the per-request value when it is not <c>0</c>, otherwise the default.
/// </description></item>
/// <item><description>
/// <see cref="ThrowOnError"/> and <see cref="UseJsonContentType"/> always take the per-request value.
/// </description></item>
/// <item><description>
/// The <see cref="DefaultRequestHeaders"/> of both are sent. For a header present in both, the
/// per-request value is used.
/// </description></item>
/// </list>
/// <para>
/// A new instance starts with 30, 3, 200 and 10000 in the numeric properties, <c>false</c> in
/// <see cref="ThrowOnError"/> and <c>true</c> in <see cref="UseJsonContentType"/>. Passing one therefore
/// replaces the registered values of those properties, unless they are set again on it. A per-request
/// <see cref="RetryCount"/> of <c>0</c> does not disable retries: it falls back to the registered default.
/// </para>
/// <para>
/// The resulting <see cref="BaseAddress"/> is applied only when the client that the factory returns has
/// none. A base address configured on a named client is kept.
/// </para>
/// </remarks>
public class HttpRequestOptions
{
    /// <summary>Gets or sets base address for created HttpClient if specified (can be null when using full URLs).</summary>
    public string? BaseAddress { get; set; }

    /// <summary>Gets or sets default timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>Gets or sets number of retry attempts on transient failures (0 = no retry).</summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>Gets or sets base delay in milliseconds used for exponential backoff.</summary>
    public int RetryBaseDelayMs { get; set; } = 200; // 200ms base

    /// <summary>Gets or sets maximum allowed delay between retries in ms.</summary>
    public int MaxRetryDelayMs { get; set; } = 10000; // 10s

    /// <summary>Gets or sets a value indicating whether if true, methods throw HttpClientException on final failure instead of returning unsuccessful HttpResponse&lt;T&gt;.</summary>
    public bool ThrowOnError { get; set; } = false;

    /// <summary>Gets or sets default request headers to add to each request if present.</summary>
    public IDictionary<string, string>? DefaultRequestHeaders { get; set; }

    /// <summary>Gets or sets optional specific named client to create from IHttpClientFactory. If null, factory.CreateClient() is used.</summary>
    public string? NamedHttpClient { get; set; }

    /// <summary>Gets or sets a value indicating whether if true, sends content type application/json when body present.</summary>
    public bool UseJsonContentType { get; set; } = true;

    /// <summary>Gets or sets optional serializer override per-request. If null will use DI resolved one.</summary>
    public IJsonSerializer? Serializer { get; set; }
}
