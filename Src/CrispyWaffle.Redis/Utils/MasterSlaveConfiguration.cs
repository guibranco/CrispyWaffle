using System;
using CrispyWaffle.Configuration;

namespace CrispyWaffle.Redis.Utils
{
    /// <summary>
    /// Represents the configuration for a Redis master/slave connection setup.
    /// This class is used to configure the connection details for both the Redis master and slave nodes.
    /// </summary>
    /// <remarks>
    /// The master and slave connections are identified by the <see cref="ConnectionName"/> attributes.
    /// This configuration ensures that both master and slave nodes are connected properly by providing
    /// the list of hosts and optionally the shared password for authentication.
    /// </remarks>
    [ConnectionName("RedisMaster", Order = 0), ConnectionName("RedisSlave", Order = 1)]
    public sealed class MasterSlaveConfiguration
    {
        /// <summary>
        /// Gets the concatenated list of host addresses for the master and slave Redis instances.
        /// The format is "host:port,host:port", where each pair corresponds to the master and slave nodes.
        /// </summary>
        /// <value>The concatenated hosts list in the format "host:port,host:port".</value>
        public string HostsList { get; }

        /// <summary>
        /// Gets the password used for authentication with the Redis nodes.
        /// This value is taken from either the master or the slave credentials.
        /// </summary>
        /// <value>The password for Redis connection, or <see langword="null"/> if no credentials are provided.</value>
        public string Password { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterSlaveConfiguration"/> class.
        /// </summary>
        /// <param name="master">The master Redis connection configuration.</param>
        /// <param name="slave">The slave Redis connection configuration.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if either <paramref name="master"/> or <paramref name="slave"/> is <see langword="null"/>.
        /// </exception>
        /// <remarks>
        /// This constructor combines the host and port information of the master and slave connections into a single
        /// comma-separated string representing the hosts list. It also sets the password if provided by either the
        /// master or slave credentials.
        /// </remarks>
        public MasterSlaveConfiguration(IConnection master, IConnection slave)
        {
            if (master == null)
            {
                throw new ArgumentNullException(
                    nameof(master),
                    "Master connection cannot be null."
                );
            }

            if (slave == null)
            {
                throw new ArgumentNullException(nameof(slave), "Slave connection cannot be null.");
            }

            HostsList = string.Concat(
                master.Host,
                @":",
                master.Port,
                @",",
                slave.Host,
                @":",
                slave.Port
            );

            if (master.Credentials != null || slave.Credentials != null)
            {
                Password = master.Credentials?.Password ?? slave.Credentials?.Password;
            }
        }
    }
}
