namespace CrispyWaffle.Notification
{
    using Composition;
    using Extensions;
    using Log;
    using Model;
    using Providers;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The notification consumer class.
    /// </summary>

    public static class NotificationConsumer
    {
        #region Private fields

        /// <summary>
        /// The list of notification providers to be used by this instance
        /// </summary>
        private static readonly List<INotificationProvider> Providers;

        #endregion

        #region ~Ctor

        /// <summary>
        /// Static initializer of notification consumer class.
        /// </summary>
        static NotificationConsumer()
        {
            Providers = new List<INotificationProvider>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Adds the provider.
        /// </summary>
        /// <typeparam name="TINotificationProvider">The type of the i notification provider.</typeparam>
        /// <returns></returns>
        public static INotificationProvider AddProvider<TINotificationProvider>()
            where TINotificationProvider : INotificationProvider
        {
            var provider = ServiceLocator.Resolve<TINotificationProvider>();
            Providers.Add(provider);
            return provider;
        }

        /// <summary>
        /// Adds a provider to the providers list
        /// </summary>
        /// <param name="provider"><see cref="INotificationProvider"/></param>
        /// <returns>The <paramref name="provider"/></returns>
        public static INotificationProvider AddProvider(INotificationProvider provider)
        {
            Providers.Add(provider);
            return provider;
        }

        /// <summary>
        /// Sends a notification using a exclusive provider, only if the provider is in the providers list
        /// </summary>
        /// <typeparam name="TNotificationProvider">The <see cref="INotificationProvider"/> that will delivery the notification</typeparam>
        ///<param name="receiver"><see cref="INotificationReceiver"/></param>
        /// <param name="notification"><see cref="INotification"/></param>
        /// <returns>true if the provider exists in the providers lists or false if not</returns>
        public static bool Notify<TNotificationProvider>(INotificationReceiver receiver, INotification notification) where TNotificationProvider : INotificationProvider
        {
            var type = typeof(TNotificationProvider);
            var provider = Providers.SingleOrDefault(p => type == p.GetType());
            if (provider == null)
                return false;
            provider.Notify(receiver, notification, Guid.NewGuid());
            return true;
        }

        /// <summary>
        /// Sends a notification using a exclusive provider, only if the provider is in  the providers list.
        /// </summary>
        /// <typeparam name="TNotificationProvider">The <see cref="INotificationProvider"/> that will delivery the notification</typeparam>
        /// <param name="receivers">A list of <see cref="INotificationReceiver"/></param>
        /// <param name="notification"><see cref="INotification"/></param>
        /// <returns>true if the provider exists in the providers lists or false if not</returns>
        public static bool Notify<TNotificationProvider>(INotificationReceiver[] receivers, INotification notification) where TNotificationProvider : INotificationProvider
        {
            var type = typeof(TNotificationProvider);
            var provider = Providers.SingleOrDefault(p => type == p.GetType());
            if (provider == null)
                return false;
            provider.Notify(receivers, notification, Guid.NewGuid());
            return true;
        }

        /// <summary>
        /// Send a notification to all registered providers
        /// </summary>
        /// <param name="notification"><see cref="INotification"/></param>
        public static void Notify(INotification notification)
        {
            var identifier = Guid.NewGuid();
            LogConsumer.Info("Notification {0} with identifier {1}", notification.GetNotificationType(), identifier);
            foreach (var provider in Providers)
                provider.Notify(notification, identifier);
        }

        #endregion
    }
}
