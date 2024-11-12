using System.ComponentModel;
using System.Xml.Serialization;
using CrispyWaffle.Serialization;

namespace CrispyWaffle.Configuration;

/// <summary>
/// Class Connection. This class cannot be inherited.
/// Implements the <see cref="IConnection" />.
/// </summary>
/// <seealso cref="IConnection" />
[Serializer]
public sealed class Connection : IConnection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Connection"/> class.
    /// </summary>
    public Connection() => Credentials = new Credentials();

    /// <summary>
    /// Gets or sets the connection name/identifier.
    /// </summary>
    /// <value>The connection name/identifier.</value>
    [XmlAttribute(AttributeName = "Name")]
    [Localizable(false)]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the host.
    /// </summary>
    /// <value>The host.</value>
    [Localizable(false)]
    public string Host { get; set; }

    /// <summary>
    /// Gets or sets the port.
    /// </summary>
    /// <value>The port.</value>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets the credentials.
    /// </summary>
    /// <value>The credentials.</value>
    [XmlIgnore]
    public IConnectionCredential Credentials { get; set; }

    /// <summary>
    /// Gets or sets the credentials internal.
    /// </summary>
    /// <value>The credentials internal.</value>
    [XmlElement("Credentials")]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public Credentials CredentialsInternal
    {
        get => (Credentials)Credentials;
        set => Credentials = value;
    }
}
