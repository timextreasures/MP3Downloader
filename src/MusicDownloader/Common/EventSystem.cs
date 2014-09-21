using System;
using Microsoft.Practices.Prism.PubSubEvents;

namespace MusicDownloader.Common
{
    /// <summary>
    ///     Static EventAggregator
    /// </summary>
    public static class EventSystem
    {
        private static IEventAggregator current;

        public static IEventAggregator Current
        {
            get { return current ?? (current = new EventAggregator()); }
        }

        private static PubSubEvent<TEvent> GetEvent<TEvent>()
        {
            return Current.GetEvent<PubSubEvent<TEvent>>();
        }

        public static void Publish<TEvent>(TEvent @event = default(TEvent))
        {
            GetEvent<TEvent>().Publish(@event);
        }

        public static SubscriptionToken Subscribe<TEvent>(Action action,
            ThreadOption threadOption = ThreadOption.PublisherThread, bool keepSubscriberReferenceAlive = false)
        {
            return Subscribe<TEvent>(e => action(), threadOption, keepSubscriberReferenceAlive);
        }

        public static SubscriptionToken Subscribe<TEvent>(Action<TEvent> action,
            ThreadOption threadOption = ThreadOption.PublisherThread, bool keepSubscriberReferenceAlive = false,
            Predicate<TEvent> filter = null)
        {
            return GetEvent<TEvent>().Subscribe(action, threadOption, keepSubscriberReferenceAlive, filter);
        }

        public static void Unsubscribe<TEvent>(SubscriptionToken token)
        {
            GetEvent<TEvent>().Unsubscribe(token);
        }

        public static void Unsubscribe<TEvent>(Action<TEvent> subscriber)
        {
            GetEvent<TEvent>().Unsubscribe(subscriber);
        }
    }
}