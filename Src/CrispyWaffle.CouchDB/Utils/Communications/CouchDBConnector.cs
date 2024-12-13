using System;
using CouchDB.Driver;
using CrispyWaffle.Configuration;

namespace CrispyWaffle.CouchDB.Utils.Communications;

/// <summary>
/// Represents a connector for interacting with a CouchDB instance.
/// Implements the <see cref="IDisposable"/> interface to ensure proper cleanup of resources.
/// </summary>
/// <seealso cref="IDisposable"/>
[ConnectionName("CouchDB")]
public class CouchDBConnector : IDisposable
{
    /// <summary>
    /// Gets the <see cref="CouchClient"/> used for interacting with the CouchDB database.
    /// </summary>
    /// <value>The <see cref="CouchClient"/> instance.</value>
    public CouchClient CouchDBClient { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CouchDBConnector"/> class with a specified connection.
    /// </summary>
    /// <param name="connection">The <see cref="IConnection"/> object containing the connection details such as host, port, and credentials.</param>
    /// <remarks>
    /// This constructor sets up a new <see cref="CouchClient"/> instance using the provided connection information, including
    /// basic authentication for secure access.
    /// </remarks>
    public CouchDBConnector(IConnection connection) =>
        CouchDBClient = new CouchClient(
            $"{connection.Host}:{connection.Port}",
            s =>
                s.UseBasicAuthentication(
                    connection.Credentials.Username,
                    connection.Credentials.Password
                )
        );

    /// <summary>
    /// Initializes a new instance of the <see cref="CouchDBConnector"/> class with an existing <see cref="CouchClient"/>.
    /// </summary>
    /// <param name="client">An already configured <see cref="CouchClient"/> instance.</param>
    /// <remarks>
    /// This constructor allows you to provide a pre-existing <see cref="CouchClient"/> for connecting to CouchDB.
    /// </remarks>
    public CouchDBConnector(CouchClient client) => CouchDBClient = client;

    /// <inheritdoc/>
    /// <summary>
    /// Disposes of the resources used by the <see cref="CouchDBConnector"/> class.
    /// </summary>
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
    /// This method is part of the <see cref="IDisposable"/> pattern. When disposing is true, it releases both managed and unmanaged resources.
    /// If disposing is false, only unmanaged resources are released. This helps ensure that resources are freed appropriately, especially in cases involving unmanaged resources such as database connections.
    /// </remarks>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            CouchDBClient?.Dispose();
        }
    }
}
