using System.ComponentModel;

namespace CrispyWaffle.Configuration
{
    /// <summary>
    /// Interface IConnection.
    /// </summary>
    public interface IConnection
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        /// <value>The host.</value>
        [Localizable(false)]
        string Host { get; set; }

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>The port.</value>
        int Port { get; set; }

        /// <summary>
        /// Gets or sets the credentials.
        /// </summary>
        /// <value>The credentials.</value>
        IConnectionCredential Credentials { get; set; }
    }
}
