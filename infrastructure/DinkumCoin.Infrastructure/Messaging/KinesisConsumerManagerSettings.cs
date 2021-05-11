using System.Collections.Generic;

namespace DinkumCoin.Infrastructure.Messaging
{
    public class KinesisConsumerManagerSettings
    {
        public List<string> StreamTopics { get; set; }
    }
}
