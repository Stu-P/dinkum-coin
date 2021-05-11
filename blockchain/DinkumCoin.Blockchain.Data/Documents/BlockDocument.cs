using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using DinkumCoin.Domain.Models;

namespace DinkumCoin.Blockchain.Data.Documents
{
    [DynamoDBTable("BlockchainStore")]

    public class BlockDocument
    {

        [DynamoDBHashKey]
        public long BlockId { get; set; }

        [DynamoDBProperty(typeof(UtcDateTimeConverter))]
        public DateTime TimeStamp { get; set; }

        [DynamoDBProperty]
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();

        [DynamoDBProperty]
        public string PreviousHash { get; set; }

        [DynamoDBProperty]
        public long Proof { get; set; }

        [DynamoDBProperty]
        public long? CurrentIndex { get; set; }
    }
}
