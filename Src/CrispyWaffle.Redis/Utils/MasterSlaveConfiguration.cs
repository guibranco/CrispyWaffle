using System;
using CrispyWaffle.Configuration;

namespace CrispyWaffle.Redis.Utils
{
    /// <summary>
    /// The Redis master/slave connection configuration class.
    /// </summary>
    [ConnectionName("RedisMaster", Order = 0), ConnectionName("RedisSlave", Order = 1)]
    public sealed class MasterSlaveConfiguration
    {
        /// <summary>
        /// Gets the hosts list.
        /// </summary>
        /// <value>The hosts list.</value>
        public string HostsList { get; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>The password.</value>
        public string Password { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MasterSlaveConfiguration" /> class.
        /// </summary>
        /// <param name="master">The master.</param>
        /// <param name="slave">The slave.</param>
        /// <exception cref="ArgumentNullException">master</exception>
        /// <exception cref="ArgumentNullException">slave</exception>
        public MasterSlaveConfiguration(IConnection master, IConnection slave)
        {
            if (master == null)
            {
                throw new ArgumentNullException(nameof(master));
            }

            if (slave == null)
            {
                throw new ArgumentNullException(nameof(slave));
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
                Password = master.Credentials?.Password ?? slave.Credentials.Password;
            }
        }
    }
}
