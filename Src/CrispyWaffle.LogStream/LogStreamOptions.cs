using System;
using System.Collections.Generic;

namespace CrispyWaffle.LogStream;

/// <summary>
/// Settings for sending CrispyWaffle log messages to a LogStream server.
/// </summary>
/// <remarks>
/// LogStream accepts two kinds of write credentials:
/// <list type="bullet">
/// <item><description>File storage: a single shared secret, sent as <c>Authorization: Bearer &lt;API_SECRET&gt;</c>. Set <see cref="ApiSecret"/> and <see cref="AppKey"/>.</description></item>
/// <item><description>MariaDB storage: a per-client key and token, sent as <c>X-Api-Key</c> and <c>X-Api-Token</c>. Set <see cref="ApiKey"/> and <see cref="ApiToken"/>; the server uses the key as the app key.</description></item>
/// </list>
/// </remarks>
public sealed class LogStreamOptions
{
    /// <summary>
    /// Gets or sets the base address of the LogStream server, for example <c>https://logs.example.com</c>.
    /// The provider posts to <c>{ServerUrl}/api/logs</c>.
    /// </summary>
    public Uri ServerUrl { get; set; }

    /// <summary>
    /// Gets or sets the application slug (<c>app_key</c>). Required with <see cref="ApiSecret"/>;
    /// ignored by the server when <see cref="ApiKey"/>/<see cref="ApiToken"/> are used.
    /// </summary>
    public string AppKey { get; set; }

    /// <summary>
    /// Gets or sets the instance or environment identifier (<c>app_id</c>), for example <c>production</c>.
    /// </summary>
    public string AppId { get; set; }

    /// <summary>
    /// Gets or sets the shared write secret (file storage mode).
    /// </summary>
    public string ApiSecret { get; set; }

    /// <summary>
    /// Gets or sets the client key (MariaDB storage mode), sent as <c>X-Api-Key</c>.
    /// </summary>
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the client token (MariaDB storage mode), sent as <c>X-Api-Token</c>.
    /// </summary>
    public string ApiToken { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of entries sent in one request. Defaults to 100.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets how long the provider waits before sending a partial batch. Defaults to 2 seconds.
    /// </summary>
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets or sets the maximum number of entries waiting to be sent. New entries are dropped while
    /// the queue is full. Defaults to 10,000.
    /// </summary>
    public int QueueLimit { get; set; } = 10_000;

    /// <summary>
    /// Gets or sets how many times a batch is retried after a transient failure (network error,
    /// timeout, HTTP 408, 429 or 5xx) before it is dropped. Defaults to 3.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay before the first retry; it doubles after each attempt. Defaults to 1 second.
    /// </summary>
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Gets or sets the HTTP request timeout. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets how long <see cref="Log.LogStreamLogProvider.Dispose()"/> waits for queued
    /// entries to be sent. Defaults to 10 seconds.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Gets or sets the LogStream level used for <c>Trace</c> calls. Defaults to <c>debug</c>.
    /// </summary>
    public string TraceLevel { get; set; } = LogStreamLevels.Debug;

    /// <summary>
    /// Gets or sets the LogStream level used for <c>Trace</c> calls that carry an exception.
    /// Defaults to <c>error</c>, so exceptions are not hidden among debug entries.
    /// </summary>
    public string ExceptionLevel { get; set; } = LogStreamLevels.Error;

    /// <summary>
    /// Gets or sets a value indicating whether host, process and thread details are added to each
    /// entry's context. Defaults to <c>true</c>.
    /// </summary>
    public bool IncludeEnvironment { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the current <see cref="System.Diagnostics.Activity"/>
    /// trace ID is sent as the entry's <c>trace_id</c>. Defaults to <c>true</c>.
    /// </summary>
    public bool UseActivityTraceId { get; set; } = true;

    /// <summary>
    /// Gets the properties added to the context of every entry.
    /// </summary>
    public IDictionary<string, object> StaticContext { get; } =
        new Dictionary<string, object>(StringComparer.Ordinal);

    /// <summary>
    /// Gets or sets a callback invoked when a batch is dropped or the queue is full.
    /// Log providers must not log through <c>LogConsumer</c> from here, as that would loop back.
    /// </summary>
    public Action<Exception> OnError { get; set; }

    /// <summary>
    /// Gets a value indicating whether the shared-secret (Bearer) mode is used.
    /// </summary>
    internal bool UsesSharedSecret => !string.IsNullOrWhiteSpace(ApiSecret);

    /// <summary>
    /// Validates the settings.
    /// </summary>
    /// <exception cref="InvalidOperationException">A required setting is missing or out of range.</exception>
    public void Validate()
    {
        ValidateEndpoint();
        ValidateCredentials();
        ValidateBatching();
        ValidateTimings();
        ValidateLevels();
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    private static bool IsTimerSafe(TimeSpan value) => value.TotalMilliseconds <= int.MaxValue;

    private void ValidateEndpoint()
    {
        Require(
            ServerUrl != null && ServerUrl.IsAbsoluteUri,
            "An absolute LogStream server URL is required."
        );
        Require(!string.IsNullOrWhiteSpace(AppId), "The LogStream app_id is required.");
    }

    private void ValidateCredentials()
    {
        var hasClientCredentials =
            !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(ApiToken);

        Require(
            UsesSharedSecret || hasClientCredentials,
            "Set either ApiSecret (file storage) or both ApiKey and ApiToken (MariaDB storage)."
        );
        Require(
            !UsesSharedSecret || !string.IsNullOrWhiteSpace(AppKey),
            "The LogStream app_key is required with ApiSecret."
        );
    }

    private void ValidateBatching()
    {
        Require(BatchSize >= 1 && BatchSize <= 1000, "BatchSize must be between 1 and 1000.");
        Require(QueueLimit >= BatchSize, "QueueLimit must be at least BatchSize.");
        Require(MaxRetries >= 0, "MaxRetries cannot be negative.");
    }

    private void ValidateTimings()
    {
        Require(
            FlushInterval > TimeSpan.Zero && IsTimerSafe(FlushInterval),
            "FlushInterval must be positive and at most int.MaxValue milliseconds (about 24.8 days)."
        );
        Require(
            ShutdownTimeout >= TimeSpan.Zero && IsTimerSafe(ShutdownTimeout),
            "ShutdownTimeout cannot be negative or above int.MaxValue milliseconds."
        );
    }

    private void ValidateLevels()
    {
        Require(LogStreamLevels.IsValid(TraceLevel), "TraceLevel is not a LogStream level.");
        Require(
            LogStreamLevels.IsValid(ExceptionLevel),
            "ExceptionLevel is not a LogStream level."
        );
    }
}
