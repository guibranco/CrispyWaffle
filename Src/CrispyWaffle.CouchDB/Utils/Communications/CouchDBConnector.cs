using System;
using CouchDB.Driver;
using CrispyWaffle.Configuration;

namespace CrispyWaffle.CouchDB.Utils.Communications;

/// <summary>
/// Class CouchDBConnector.
/// Implements the <see cref="IDisposable" />
/// </summary>
/// <seealso cref="IDisposable" />
[ConnectionName("CouchDB")]
public class CouchDBConnector : IDisposable
{
    /// <summary>
    /// Gets the couch database client.
    /// </summary>
    /// <value>The couch database client.</value>
    public CouchClient CouchDBClient { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CouchDBConnector"/> class.
    /// </summary>
    /// <param name="connection">The connection.</param>
    public CouchDBConnector(IConnection connection)
    {
        CouchDBClient = new CouchClient(
            $"{connection.Host}:{connection.Port}",
            s => s.UseBasicAuthentication(
                connection.Credentials.Username,
                connection.Credentials.Password
            ));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CouchDBConnector"/> class.
    /// </summary>
    /// <param name="client">The client.</param>
    public CouchDBConnector(CouchClient client)
    {
        CouchDBClient = client;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources.
    /// </summary>
    /// <param name="disposing">True if called explicitly.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            CouchDBClient?.Dispose();
        }
    }
}
