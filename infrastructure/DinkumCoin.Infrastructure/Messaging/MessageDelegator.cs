using System;
using System.Threading.Tasks;
using DinkumCoin.Domain.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DinkumCoin.Infrastructure.Messaging
{
    public class MessageDelegator
    {
        private readonly EventSubscriptionManager _subsManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MessageDelegator> _logger;

        public MessageDelegator(EventSubscriptionManager subsManager, IServiceProvider serviceProvider, ILogger<MessageDelegator> logger)
        {
            _serviceProvider = serviceProvider;
            _subsManager = subsManager ?? throw new ArgumentNullException(nameof(subsManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task DelegateMessageToHandler(string payload)
        {
            try
            {
                var key = GetMessageTypeKey(payload);
                if (string.IsNullOrWhiteSpace(key))
                {
                    _logger.LogError("Unable to determine event type, payload possiby corrupt, discarding {message}", payload);
                }

                if (!_subsManager.HasSubscription(key))
                {
                    _logger.LogInformation("No handler subscribed to message type {key}, discarding message", key);
                    return;
                }

                var eventType = _subsManager.GetEventType(key);
                var typedEvent = JsonConvert.DeserializeObject(payload, eventType) as IntegrationEvent;
                var handlerType = _subsManager.GetHandler(eventType);
                using (var scope = _serviceProvider.CreateScope())
                {
                    dynamic handler = scope.ServiceProvider.GetService(handlerType);
                    await handler.Handle((dynamic)typedEvent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured while handling event, discarding record {payload}", payload);
            }
        }

        private string GetMessageTypeKey(string messagePayload)
        {
            var baseTyped = JsonConvert.DeserializeObject<IntegrationEvent>(messagePayload);
            return baseTyped.EventType;
        }
    }
}
