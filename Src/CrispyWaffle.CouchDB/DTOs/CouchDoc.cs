using System;
using CouchDB.Driver.Types;

namespace CrispyWaffle.CouchDB.DTOs;

/// <summary>
/// Base class for persisting to CouchDB.
/// </summary>
public class CouchDoc : CouchDocument
{
    /// <summary>
    /// Gets or Sets a uniquely identifiable key.
    /// </summary>
    public string Key
    {
        get
        {
            return Id;
        }

        set
        {
            Id = value;
        }
    }

    /// <summary>
    /// Gets or Sets the sub key.
    /// </summary>
    public string SubKey { get; set; }

    /// <summary>
    /// Gets or Sets the time when the value persisted expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Gets or Sets the time to live.
    /// </summary>
    public TimeSpan TTL { get; set; }
}
