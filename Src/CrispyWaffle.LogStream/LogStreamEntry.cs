using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CrispyWaffle.LogStream;

/// <summary>
/// One entry of a <c>POST /api/logs</c> batch.
/// </summary>
internal sealed class LogStreamEntry
{
    /// <summary>
    /// Gets or sets the trace identifier; the server generates one when it is missing.
    /// </summary>
    [JsonProperty("trace_id", NullValueHandling = NullValueHandling.Ignore)]
    public string TraceId { get; set; }

    /// <summary>
    /// Gets or sets the level.
    /// </summary>
    [JsonProperty("level")]
    public string Level { get; set; }

    /// <summary>
    /// Gets or sets the category (at most 100 characters).
    /// </summary>
    [JsonProperty("category")]
    public string Category { get; set; }

    /// <summary>
    /// Gets or sets the rendered message.
    /// </summary>
    [JsonProperty("message")]
    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the template the message was rendered from, if any.
    /// </summary>
    [JsonProperty("message_template", NullValueHandling = NullValueHandling.Ignore)]
    public string MessageTemplate { get; set; }

    /// <summary>
    /// Gets or sets the structured context.
    /// </summary>
    [JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
    public IDictionary<string, object> Context { get; set; }

    /// <summary>
    /// Gets or sets when the event occurred.
    /// </summary>
    [JsonProperty("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}
