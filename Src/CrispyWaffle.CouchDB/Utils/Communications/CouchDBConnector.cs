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
            s =>
                s.UseBasicAuthentication(
                    connection.Credentials.Username,
                    connection.Credentials.Password
                )
        );
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
    /// Releases the resources used by the current instance of the class.
    /// </summary>
    /// <param name="disposing">A boolean value indicating whether the method was called directly or indirectly by a user's code.</param>
    /// <remarks>
    /// This method is part of the IDisposable pattern. When disposing is true, the method releases both managed and unmanaged resources.
    /// If disposing is false, the method only releases unmanaged resources. This allows for proper cleanup of resources when the object is no longer needed.
    /// It is important to call this method to ensure that all resources are released appropriately, especially when dealing with unmanaged resources.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            CouchDBClient?.Dispose();
        }
    }
}
