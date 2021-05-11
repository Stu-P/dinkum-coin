using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DinkumCoin.Domain.Events
{
    public class IntegrationEvent
    {
        public IntegrationEvent(string eventType, string correlationId = null)
        {
            EventId = Guid.NewGuid();
            TimeStamp = DateTime.UtcNow;
            CorrelationId = correlationId ?? Guid.NewGuid().ToString();
            EventType = eventType;

        }
        public Guid EventId { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string CorrelationId { get; private set; }
        public string EventType { get; private set; }
    }

    public static class IntegrationEventExtenstions
    {

        public static MemoryStream ToStream(this IntegrationEvent dEvent)
        {
            return new MemoryStream(Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(dEvent)));
        }
    }
}
