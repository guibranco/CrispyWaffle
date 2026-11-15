using System;
using System.Globalization;
using System.Net.Http;
using CrispyWaffle.Configuration;

namespace CrispyWaffle.LogStream;

/// <summary>
/// Holds the settings and the HTTP client used to reach a LogStream server.
/// </summary>
[ConnectionName("LogStream")]
public sealed class LogStreamConnector
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamConnector"/> class.
    /// </summary>
    /// <param name="options">The settings.</param>
    public LogStreamConnector(LogStreamOptions options)
        : this(options, null) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamConnector"/> class with a custom HTTP client,
    /// for example one created by <c>IHttpClientFactory</c> or one wrapping a test handler.
    /// </summary>
    /// <param name="options">The settings.</param>
    /// <param name="httpClient">The HTTP client, or <c>null</c> to create one.</param>
    /// <exception cref="ArgumentNullException">options.</exception>
    public LogStreamConnector(LogStreamOptions options, HttpClient httpClient)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Options.Validate();
        HttpClient = httpClient ?? new HttpClient { Timeout = Options.RequestTimeout };
        IngestUri = new Uri(EnsureTrailingSlash(Options.ServerUrl), "api/logs");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogStreamConnector"/> class from a CrispyWaffle connection.
    /// </summary>
    /// <param name="connection">
    /// The connection: <see cref="IConnection.Host"/> is the server host (a full URL is also accepted),
    /// <see cref="IConnection.Port"/> its port (443 means HTTPS). When the credentials have a user name,
    /// it is sent as <c>X-Api-Key</c> and the password as <c>X-Api-Token</c>; with only a password,
    /// the password is the shared <c>API_SECRET</c>.
    /// </param>
    /// <param name="appKey">The application slug.</param>
    /// <param name="appId">The instance or environment identifier.</param>
    /// <exception cref="ArgumentNullException">connection.</exception>
    public LogStreamConnector(IConnection connection, string appKey, string appId)
        : this(FromConnection(connection, appKey, appId)) { }

    /// <summary>
    /// Gets the settings.
    /// </summary>
    public LogStreamOptions Options { get; }

    /// <summary>
    /// Gets the HTTP client.
    /// </summary>
    public HttpClient HttpClient { get; }

    /// <summary>
    /// Gets the ingest endpoint, <c>{ServerUrl}/api/logs</c>.
    /// </summary>
    public Uri IngestUri { get; }

    private static Uri EnsureTrailingSlash(Uri uri) =>
        uri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal)
            ? uri
            : new Uri(uri.AbsoluteUri + "/");

    private static LogStreamOptions FromConnection(
        IConnection connection,
        string appKey,
        string appId
    )
    {
        if (connection == null)
        {
            throw new ArgumentNullException(
                nameof(connection),
                string.Format(
                    CultureInfo.CurrentCulture,
                    "A valid instance of {0} is required to initialize {1}!",
                    typeof(IConnection).FullName,
                    typeof(LogStreamConnector).FullName
                )
            );
        }

        Uri serverUrl;
        if (
            Uri.TryCreate(connection.Host, UriKind.Absolute, out var absolute)
            && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps)
        )
        {
            serverUrl = absolute;
        }
        else
        {
            var port = connection.Port > 0 ? connection.Port : 443;
            serverUrl = new UriBuilder(port == 443 ? "https" : "http", connection.Host, port).Uri;
        }

        var options = new LogStreamOptions
        {
            ServerUrl = serverUrl,
            AppKey = appKey,
            AppId = appId,
        };

        var credentials = connection.Credentials;
        if (credentials != null)
        {
            if (string.IsNullOrWhiteSpace(credentials.Username))
            {
                options.ApiSecret = credentials.Password;
            }
            else
            {
                options.ApiKey = credentials.Username;
                options.ApiToken = credentials.Password;
            }
        }

        return options;
    }
}
