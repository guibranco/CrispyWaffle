using System.Collections.Generic;
using Newtonsoft.Json;

namespace CrispyWaffle.LogStream;

/// <summary>
/// The <c>POST /api/logs</c> batch payload.
/// </summary>
internal sealed class LogStreamBatch
{
    /// <summary>
    /// Gets or sets the application slug.
    /// </summary>
    [JsonProperty("app_key", NullValueHandling = NullValueHandling.Ignore)]
    public string AppKey { get; set; }

    /// <summary>
    /// Gets or sets the instance identifier.
    /// </summary>
    [JsonProperty("app_id")]
    public string AppId { get; set; }

    /// <summary>
    /// Gets or sets the batch identifier.
    /// </summary>
    [JsonProperty("batch_id")]
    public string BatchId { get; set; }

    /// <summary>
    /// Gets or sets the entries.
    /// </summary>
    [JsonProperty("logs")]
    public IList<LogStreamEntry> Logs { get; set; }
}
