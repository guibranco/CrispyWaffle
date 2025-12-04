using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CrispyWaffle.HttpClient
{
    /// <summary>
    /// Small retry helper with exponential backoff + jitter and handling Retry-After header for 429.
    /// No external dependency required.
    /// </summary>
    internal static class RetryPolicy
    {
        public static async Task<(
            HttpResponseMessage? Response,
            Exception? Exception
        )> ExecuteAsync(
            Func<CancellationToken, Task<HttpResponseMessage>> action,
            int retryCount,
            int baseDelayMs,
            int maxDelayMs,
            CancellationToken ct
        )
        {
            if (retryCount <= 0)
            {
                try
                {
                    var r = await action(ct).ConfigureAwait(false);
                    return (r, null);
                }
                catch (Exception ex)
                {
                    return (null, ex);
                }
            }

            var rnd = Random.Shared;
            Exception? lastEx = null;

            for (int attempt = 0; attempt <= retryCount; attempt++)
            {
                try
                {
                    var response = await action(ct).ConfigureAwait(false);

                    if (!ShouldRetryResponse(response))
                    {
                        return (response, null);
                    }

                    // Will retry
                    var delay = ComputeDelay(attempt, baseDelayMs, maxDelayMs, rnd);

                    // If 429 and Retry-After header present, try to honor it
                    if (response.StatusCode == (HttpStatusCode)429)
                    {
                        var ra = GetRetryAfterDelay(response);
                        if (ra.HasValue)
                            delay = Math.Min((int)ra.Value.TotalMilliseconds, maxDelayMs);
                    }

                    await Task.Delay(delay, ct).ConfigureAwait(false);
                    continue;
                }
                catch (Exception ex) when (IsTransientException(ex))
                {
                    lastEx = ex;
                    if (attempt == retryCount)
                        break;

                    var delay = ComputeDelay(attempt, baseDelayMs, maxDelayMs, rnd);
                    await Task.Delay(delay, ct).ConfigureAwait(false);
                    continue;
                }
            }

            return (null, lastEx);
        }

        private static bool ShouldRetryResponse(HttpResponseMessage response)
        {
            if (response == null)
                return true;
            var code = (int)response.StatusCode;
            // Retry for 5xx, 408 Request Timeout, 429 Too Many Requests
            if (code >= 500 || code == 408 || code == 429)
                return true;
            return false;
        }

        private static bool IsTransientException(Exception ex)
        {
            // HttpRequestException and TaskCanceled (timeout) are considered transient
            return ex is HttpRequestException
                || ex is OperationCanceledException
                || ex is TimeoutException;
        }

        private static int ComputeDelay(int attempt, int baseDelayMs, int maxDelayMs, Random rnd)
        {
            // exponential backoff with jitter
            // formula: min(maxDelay, base * 2^attempt) +/- jitter up to 20%
            var exponential = baseDelayMs * (1 << Math.Min(attempt, 10)); // cap shift
            var capped = Math.Min(exponential, maxDelayMs);
            var jitter = (int)(capped * 0.2 * (rnd.NextDouble() - 0.5)); // ±10%
            var result = Math.Max(0, capped + jitter);
            return result;
        }

        private static TimeSpan? GetRetryAfterDelay(HttpResponseMessage response)
        {
            // 1. Chuẩn HTTP (HttpResponseHeaders)
            if (response.Headers?.RetryAfter != null)
            {
                var ra = response.Headers.RetryAfter;
                if (ra.Delta.HasValue)
                    return ra.Delta.Value;
                if (ra.Date.HasValue)
                    return ra.Date.Value - DateTimeOffset.UtcNow;
            }

            // 2. Fallback - parse thủ công từ mọi header (cả ContentHeaders)
            if (
                response.Headers.TryGetValues("Retry-After", out var headerValues)
                || (
                    response.Content?.Headers?.TryGetValues("Retry-After", out headerValues)
                    ?? false
                )
            )
            {
                var retryValue = headerValues.FirstOrDefault();
                if (int.TryParse(retryValue, out var seconds))
                    return TimeSpan.FromSeconds(seconds);

                if (DateTimeOffset.TryParse(retryValue, out var retryDate))
                    return retryDate - DateTimeOffset.UtcNow;
            }

            return null;
        }
    }
}
