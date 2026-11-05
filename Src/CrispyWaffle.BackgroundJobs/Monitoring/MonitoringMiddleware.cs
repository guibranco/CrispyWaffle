using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace CrispyWaffle.BackgroundJobs.Monitoring
{
    /// <summary>
    /// ASP.NET Core middleware that serves the <see cref="JobMetrics"/> counters as JSON at <c>/jobs/metrics</c>.
    /// </summary>
    public class MonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JobMetrics _metrics;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitoringMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="metrics">The counters to serve.</param>
        public MonitoringMiddleware(RequestDelegate next, JobMetrics metrics)
        {
            _next = next;
            _metrics = metrics;
        }

        /// <summary>
        /// Writes the counters as JSON when the request path starts with <c>/jobs/metrics</c>;
        /// otherwise calls the next middleware.
        /// </summary>
        /// <param name="context">The context of the current request.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/jobs/metrics"))
            {
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(_metrics.Snapshot()));
                return;
            }

            await _next(context);
        }
    }
}
