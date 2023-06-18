namespace CrispyWaffle.Redis.Utils
{
    using Composition;
    using Extensions;
    using Communications;
    using StackExchange.Redis;
    using System;

    /// <summary>
    /// The Redis extensions class.
    /// </summary>
    public static class RedisExtensions
    {
        #region Private fields

        /// <summary>
        /// The connector
        /// </summary>
        private static readonly RedisConnector _connector = ServiceLocator.Resolve<RedisConnector>();

        #endregion

        #region Public methods

        /// <summary>
        /// Temporaries increase the desired cache key in the default connector database.
        /// </summary>
        /// <param name="cacheKey">The cache key.</param>
        /// <param name="ttl">The time to live.</param>
        /// <returns>System.Int32.</returns>
        public static int TemporaryIncrease(string cacheKey, TimeSpan ttl)
        {
            _connector.DefaultDatabase.StringIncrement(cacheKey, 1, CommandFlags.FireAndForget);

            _connector.DefaultDatabase.KeyExpire(cacheKey, ttl, CommandFlags.FireAndForget);

            return _connector.DefaultDatabase.StringGet(cacheKey, CommandFlags.PreferReplica).ToString().ToInt32();
        }

        /// <summary>
        /// Flushes the database.
        /// </summary>
        /// <param name="database">The database.</param>
        public static void FlushDatabase(int database)
        {
            _connector.GetDefaultServer().FlushDatabase(database);
        }

        #endregion
    }
}
