using System;
namespace DinkumCoin.Domain.Events
{
    public class BlockCreatedEvent : IntegrationEvent
    {
        public BlockCreatedEvent(long blockIndex, Guid nodeId) : base("Block_Created")
        {
            BlockIndex = blockIndex;
            NodeId = nodeId;
        }

        public long BlockIndex { get; private set; }
        public Guid NodeId { get; private set; }
    }
}
