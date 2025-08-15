using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text.Json;
using CrispyWaffle.HttpClient.Serialization;

namespace CrispyWaffle.HttpClient
{
    /// <summary>
    /// Options used as defaults (via IOptions) and can be overriden per-request.
    /// </summary>
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
}