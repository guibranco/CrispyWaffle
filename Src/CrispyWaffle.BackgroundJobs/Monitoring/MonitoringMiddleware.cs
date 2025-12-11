using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace CrispyWaffle.BackgroundJobs.Monitoring
{
    public class MonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JobMetrics _metrics;

        public MonitoringMiddleware(RequestDelegate next, JobMetrics metrics)
        {
            _next = next;
            _metrics = metrics;
        }

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