using System;
using System.Threading.Tasks;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using DinkumCoin.Domain.Events;
using Microsoft.Extensions.Logging;

namespace DinkumCoin.Infrastructure.Messaging
{
    public class KinesisStreamPublisher : IEventPublisher
    {
        private readonly IAmazonKinesis _kinesisClient;
        private readonly ILogger<KinesisStreamPublisher> _logger;


        public KinesisStreamPublisher(IAmazonKinesis kinesisClient, ILogger<KinesisStreamPublisher> logger)
        {
            _kinesisClient = kinesisClient ?? throw new ArgumentNullException(nameof(kinesisClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        public async Task Publish(IntegrationEvent @event, string streamName)
        {
            using (_logger.BeginScope($"Publish {@event.EventType}"))
            {
                try
                {
                    var response = await _kinesisClient.PutRecordAsync(new PutRecordRequest
                    {
                        StreamName = streamName,
                        Data = @event.ToStream(),
                        PartitionKey = @event.EventType,
                    });

                    _logger.LogInformation("Event published successfully. {ShardId} {SequenceNumber}", response.ShardId, response.SequenceNumber);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Event publish failed {@event}", @event);

                }
            }
        }
    }
}
