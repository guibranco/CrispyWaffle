namespace CrispyWaffle.Log.Providers
{
    using System;

    /// <summary>
    /// Class EventIdProvider.
    /// Implements the <see cref="CrispyWaffle.Log.Providers.IEventIdProvider" />
    /// </summary>
    /// <seealso cref="CrispyWaffle.Log.Providers.IEventIdProvider" />
    public class EventIdProvider : IEventIdProvider
    {
        /// <summary>
        /// Compute a 32-bit hash of the provided <paramref name="message"/>. The
        /// resulting hash value can be uses as an event id in lieu of transmitting the
        /// full template string.
        /// </summary>
        /// <param name="message">A message template.</param>
        /// <returns>A 32-bit hash of the template.</returns>
        static int Compute(string message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            // Jenkins one-at-a-time https://en.wikipedia.org/wiki/Jenkins_hash_function
            unchecked
            {
                uint hash = 0;

                foreach (var t in message)
                {
                    hash += t;
                    hash += (hash << 10);
                    hash ^= (hash >> 6);
                }

                hash += (hash << 3);
                hash ^= (hash >> 11);
                hash += (hash << 15);

                //even though the api is type int, eventID must be between 0 and 65535
                //https://msdn.microsoft.com/en-us/library/d3159s0c(v=vs.110).aspx
                return (ushort)hash;
            }
        }

        #region Implementation of IEventIdProvider

        /// <summary>
        /// Computes the event identifier.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>System.UInt16.</returns>
        public ushort ComputeEventId(string message) => (ushort)Compute(message);

        #endregion
    }
}
