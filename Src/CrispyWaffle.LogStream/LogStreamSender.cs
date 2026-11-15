using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrispyWaffle.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrispyWaffle.LogStream;

/// <summary>
/// Queues entries in memory and sends them to <c>POST /api/logs</c> in batches from a background task.
/// </summary>
internal sealed class LogStreamSender : IDisposable
{
    private static readonly JsonSerializerSettings _jsonSettings = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        Formatting = Formatting.None,
    };

    private readonly LogStreamConnector _connector;
    private readonly LogStreamOptions _options;
    private readonly string _userAgent;
    private readonly ConcurrentQueue<LogStreamEntry> _queue = new();
    private readonly List<LogStreamEntry> _pending = new();
    private readonly SemaphoreSlim _signal = new(0, 1);
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly CancellationTokenSource _stopping = new();
    private readonly Task _worker;

    private int _count;
    private long _dropped;
    private int _queueFullReported;
    private int _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamSender"/> class and starts the background task.
    /// </summary>
    /// <param name="connector">The connector.</param>
    public LogStreamSender(LogStreamConnector connector)
    {
        _connector = connector ?? throw new ArgumentNullException(nameof(connector));
        _options = connector.Options;
        _userAgent = BuildUserAgent();
        _worker = Task.Run(RunAsync, CancellationToken.None);
    }

    /// <summary>
    /// Gets the number of entries waiting to be sent.
    /// </summary>
    public int Count => Volatile.Read(ref _count) + _pending.Count;

    /// <summary>
    /// Gets the number of entries dropped because the queue was full or a batch could not be delivered.
    /// </summary>
    public long Dropped => Interlocked.Read(ref _dropped);

    /// <summary>
    /// Adds an entry to the queue.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <returns><c>false</c> when the entry was dropped.</returns>
    public bool TryEnqueue(LogStreamEntry entry)
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            return false;
        }

        var count = Interlocked.Increment(ref _count);
        if (count > _options.QueueLimit)
        {
            Interlocked.Decrement(ref _count);
            Interlocked.Increment(ref _dropped);

            if (Interlocked.Exchange(ref _queueFullReported, 1) == 0)
            {
                Report(
                    new LogStreamException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The LogStream queue is full ({0} entries); new entries are dropped until it drains.",
                            _options.QueueLimit
                        )
                    )
                );
            }

            return false;
        }

        _queue.Enqueue(entry);

        if (count >= _options.BatchSize)
        {
            Signal();
        }

        return true;
    }

    /// <summary>
    /// Sends every queued entry.
    /// </summary>
    /// <param name="cancellationToken">
    /// Stops waiting (for the send lock, between batches and between retries). An HTTP request already
    /// started is bounded by <see cref="LogStreamOptions.RequestTimeout"/>; a batch interrupted by
    /// cancellation is kept and sent by the next flush.
    /// </param>
    /// <returns>A task that completes when the queue is empty.</returns>
    public async Task FlushAsync(CancellationToken cancellationToken)
    {
        await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_pending.Count == 0)
                {
                    TakeBatch(_pending);
                }

                if (_pending.Count == 0)
                {
                    return;
                }

                await SendWithRetryAsync(_pending, cancellationToken).ConfigureAwait(false);
                _pending.Clear();
            }
        }
        finally
        {
            _sendLock.Release();
        }
    }

    /// <summary>
    /// Stops the background task and sends what is left, waiting at most
    /// <see cref="LogStreamOptions.ShutdownTimeout"/>.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _stopping.Cancel();

        try
        {
            _worker.Wait(_options.ShutdownTimeout);
        }
        catch (AggregateException)
        {
            // The worker only ends by cancellation; nothing to report.
        }

        using (var timeout = new CancellationTokenSource(_options.ShutdownTimeout))
        {
            try
            {
                FlushAsync(timeout.Token).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                var left = Count;
                if (left > 0)
                {
                    Interlocked.Add(ref _dropped, left);
                    Report(
                        new LogStreamException(
                            string.Format(
                                CultureInfo.InvariantCulture,
                                "{0} LogStream entries were not sent before the shutdown timeout.",
                                left
                            )
                        )
                    );
                }
            }
        }

        _stopping.Dispose();
        _signal.Dispose();
    }

    private static bool IsTransient(int statusCode) =>
        statusCode == 408 || statusCode == 429 || statusCode >= 500;

    private static string BuildUserAgent()
    {
        var version =
            typeof(LogStreamSender)
                .Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion
            ?? typeof(LogStreamSender).Assembly.GetName().Version?.ToString()
            ?? "0.0.0";
        var plus = version.IndexOf('+');
        if (plus > 0)
        {
            version = version.Substring(0, plus);
        }

        var application = string.IsNullOrWhiteSpace(EnvironmentHelper.ApplicationName)
            ? "CrispyWaffle"
            : EnvironmentHelper.ApplicationName.Replace(' ', '-');

        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}/{1} (CrispyWaffle.LogStream/{2})",
            application,
            EnvironmentHelper.Version ?? "0.0.0",
            version
        );
    }

    private async Task RunAsync()
    {
        var token = _stopping.Token;

        while (!token.IsCancellationRequested)
        {
            try
            {
                await _signal.WaitAsync(_options.FlushInterval, token).ConfigureAwait(false);
                await FlushAsync(token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                return;
            }
            catch (Exception e)
            {
                Report(new LogStreamException("The LogStream background sender failed.", e));
            }
        }
    }

    private void Signal()
    {
        if (_signal.CurrentCount != 0)
        {
            return;
        }

        try
        {
            _signal.Release();
        }
        catch (SemaphoreFullException)
        {
            // Already signalled.
        }
        catch (ObjectDisposedException)
        {
            // Disposing.
        }
    }

    private void TakeBatch(List<LogStreamEntry> batch)
    {
        while (batch.Count < _options.BatchSize && _queue.TryDequeue(out var entry))
        {
            Interlocked.Decrement(ref _count);
            batch.Add(entry);
        }

        if (batch.Count > 0)
        {
            Interlocked.Exchange(ref _queueFullReported, 0);
        }
    }

    private async Task SendWithRetryAsync(
        List<LogStreamEntry> entries,
        CancellationToken cancellationToken
    )
    {
        string json;
        try
        {
            json = JsonConvert.SerializeObject(
                new LogStreamBatch
                {
                    AppKey = _options.AppKey,
                    AppId = _options.AppId,
                    BatchId = Guid.NewGuid().ToString(),
                    Logs = entries,
                },
                _jsonSettings
            );
        }
        catch (JsonException e)
        {
            Drop(
                entries.Count,
                new LogStreamException("A LogStream batch could not be serialized.", e)
            );
            return;
        }

        var delay = _options.RetryDelay;
        Exception failure = null;

        for (var attempt = 0; attempt <= _options.MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
                delay = TimeSpan.FromTicks(delay.Ticks * 2);
            }

            try
            {
                using var request = BuildRequest(json);
                using var response = await _connector
                    .HttpClient.SendAsync(request, CancellationToken.None)
                    .ConfigureAwait(false);
                var body =
                    response.Content == null
                        ? null
                        : await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var status = (int)response.StatusCode;

                if (response.IsSuccessStatusCode)
                {
                    ReportEntryErrors(status, body);
                    return;
                }

                failure = new LogStreamException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "LogStream rejected a batch of {0} entries with HTTP {1}.",
                        entries.Count,
                        status
                    ),
                    status,
                    body
                );

                if (!IsTransient(status))
                {
                    Drop(entries.Count, failure);
                    return;
                }
            }
            catch (HttpRequestException e)
            {
                failure = e;
            }
            catch (TaskCanceledException e)
            {
                // HttpClient timeout.
                failure = e;
            }
        }

        Drop(
            entries.Count,
            new LogStreamException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "A batch of {0} LogStream entries was dropped after {1} attempts.",
                    entries.Count,
                    _options.MaxRetries + 1
                ),
                (failure as LogStreamException)?.StatusCode,
                (failure as LogStreamException)?.ResponseBody,
                failure
            )
        );
    }

    private HttpRequestMessage BuildRequest(string json)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, _connector.IngestUri)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

        if (_options.UsesSharedSecret)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _options.ApiSecret
            );
        }
        else
        {
            request.Headers.TryAddWithoutValidation("X-Api-Key", _options.ApiKey);
            request.Headers.TryAddWithoutValidation("X-Api-Token", _options.ApiToken);
        }

        request.Headers.TryAddWithoutValidation("User-Agent", _userAgent);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    private void ReportEntryErrors(int status, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return;
        }

        try
        {
            if (
                JToken.Parse(body) is JObject result
                && result["errors"] is JArray errors
                && errors.Count > 0
            )
            {
                Interlocked.Add(ref _dropped, errors.Count);
                Report(
                    new LogStreamException(
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "LogStream rejected {0} entries of a batch: {1}",
                            errors.Count,
                            string.Join("; ", errors)
                        ),
                        status,
                        body
                    )
                );
            }
        }
        catch (JsonException)
        {
            // Not JSON; the batch was still accepted.
        }
    }

    private void Drop(int entries, Exception failure)
    {
        Interlocked.Add(ref _dropped, entries);
        Report(failure);
    }

    private void Report(Exception failure)
    {
        var handler = _options.OnError;
        if (handler == null)
        {
            System.Diagnostics.Trace.TraceWarning("CrispyWaffle.LogStream: {0}", failure.Message);
            return;
        }

        try
        {
            handler(failure);
        }
        catch (Exception e)
        {
            System.Diagnostics.Trace.TraceWarning(
                "CrispyWaffle.LogStream: OnError threw {0}",
                e.Message
            );
        }
    }
}
