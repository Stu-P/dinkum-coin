using System;
namespace DinkumCoin.Blockchain.Core.Services
{
    public class BlockchainServiceSettings
    {
        public Guid NodeId { get; set; }
        public int MiningReward { get; set; }
        public string TransactionTopic { get; set; }
        public string BlockchainTopic { get; set; }
    }
}

