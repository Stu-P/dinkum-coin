using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DinkumCoin.Domain.EventHandlers;
using DinkumCoin.Domain.Events;

namespace DinkumCoin.Infrastructure.Messaging
{
    public class EventSubscriptionManager
    {
        private readonly IDictionary<Type, Type> _handlers;
        private readonly IDictionary<string, Type> _eventTypes;

        public EventSubscriptionManager()
        {
            _handlers = new ConcurrentDictionary<Type, Type>();
            _eventTypes = new ConcurrentDictionary<string, Type>();
        }

        public void AddSubscription<T, TH>(string eventType)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            _eventTypes.Add(eventType, typeof(T));
            _handlers.Add(typeof(T), typeof(TH));
        }

        public bool HasSubscription(string eventType) => _eventTypes.ContainsKey(eventType);

        public Type GetHandler(Type eventType) => _handlers[eventType];

        public Type GetEventType(string eventType) => _eventTypes[eventType];

    }
}
