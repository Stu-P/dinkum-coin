using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Microsoft.Extensions.Logging;


namespace DinkumCoin.Infrastructure.Messaging
{
    public class KinesisConsumer : IEventConsumer
    {
        private readonly IAmazonKinesis _kinesisClient;
        private readonly MessageDelegator _messageDelegator;
        private readonly ILogger<KinesisConsumer> _logger;

        private const int PollIntervalMS = 10000;

        public KinesisConsumer(IAmazonKinesis kinesisClient, ILogger<KinesisConsumer> logger, MessageDelegator messageDelegator)
        {
            _kinesisClient = kinesisClient ?? throw new ArgumentNullException(nameof(kinesisClient));
            _messageDelegator = messageDelegator ?? throw new ArgumentNullException(nameof(messageDelegator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Start(string streamName, CancellationToken token = default)
        {
            Thread thread = Thread.CurrentThread;

            _logger.LogInformation("Starting consumer for {stream}");
            DescribeStreamRequest describeRequest = new DescribeStreamRequest
            {
                StreamName = streamName
            };

            DescribeStreamResponse describeResponse = await _kinesisClient.DescribeStreamAsync(describeRequest);
            List<Shard> shards = describeResponse.StreamDescription.Shards;

            foreach (Shard shard in shards)
            {
                GetShardIteratorRequest iteratorRequest = new GetShardIteratorRequest
                {
                    StreamName = streamName,
                    ShardId = shard.ShardId,
                    ShardIteratorType = ShardIteratorType.LATEST
                };

                GetShardIteratorResponse iteratorResponse = await _kinesisClient.GetShardIteratorAsync(iteratorRequest);
                string iteratorId = iteratorResponse.ShardIterator;

                while (!string.IsNullOrEmpty(iteratorId))
                {
                    GetRecordsRequest getRequest = new GetRecordsRequest
                    {
                        Limit = 1000,
                        ShardIterator = iteratorId
                    };

                    GetRecordsResponse getResponse = await _kinesisClient.GetRecordsAsync(getRequest, token);
                    string nextIterator = getResponse.NextShardIterator;
                    List<Record> records = getResponse.Records;

                    if (records.Count > 0)
                    {
                        _logger.LogInformation("Received {num} records from {stream}. ", records.Count, streamName);
                        foreach (Record record in records)
                        {
                            string payload = Encoding.UTF8.GetString(record.Data.ToArray());
                            if (!string.IsNullOrWhiteSpace(payload))
                            {
                                await _messageDelegator.DelegateMessageToHandler(payload);
                            }
                        }
                    }
                    else
                    {
                        _logger.LogInformation("No records returned from {stream}. polling again in {duration}ms", streamName, PollIntervalMS);
                    }
                    iteratorId = nextIterator;
                    await Task.Delay(PollIntervalMS);
                }
            }
        }

    }
}