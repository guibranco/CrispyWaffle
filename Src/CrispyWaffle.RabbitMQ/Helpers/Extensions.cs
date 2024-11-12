namespace CrispyWaffle.RabbitMQ.Helpers
{
    /// <summary>
    /// Class Extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the name of the exchange.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>System.String.</returns>
        public static string GetExchangeName<T>()
            where T : class, IQueuing, new()
        {
            var type = typeof(T);
            return
                type.GetCustomAttributes(typeof(ExchangeNameAttribute), true)
                    is ExchangeNameAttribute[] attributes
                && attributes.Length == 1
                ? attributes[0].ExchangeName
                : type.Name.ToLowerInvariant().Replace(@" ", @"-");
        }

        /// <summary>
        /// Gets the name of the queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>System.String.</returns>
        public static string GetQueueName<T>()
            where T : class, IQueuing, new()
        {
            var type = typeof(T);

            return
                type.GetCustomAttributes(typeof(QueueNameAttribute), true)
                    is QueueNameAttribute[] attributes
                && attributes.Length == 1
                ? attributes[0].QueueName
                : type.Name.ToLower().Replace(@" ", @"-");
        }
    }
}
