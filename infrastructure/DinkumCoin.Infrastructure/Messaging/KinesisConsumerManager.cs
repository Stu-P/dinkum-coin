using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DinkumCoin.Infrastructure.Messaging
{
    public class KinesisConsumerManager
    {
        private readonly KinesisConsumerManagerSettings _settings;
        private readonly IServiceProvider _serviceProvider;

        public KinesisConsumerManager(IServiceProvider serviceProvider, IOptions<KinesisConsumerManagerSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        public Task StartConsumers(CancellationToken token = default)
        {
            List<Task> consumers = new List<Task>();

            foreach (var stream in _settings.StreamTopics)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var consumer = scope.ServiceProvider.GetService<IEventConsumer>();
                    consumers.Add(consumer.Start(stream, token));
                }
            }
            return Task.WhenAll(consumers.ToArray());
        }
    }
}
