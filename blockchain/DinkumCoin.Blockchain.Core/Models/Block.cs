using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DinkumCoin.Domain.Models;

namespace DinkumCoin.Blockchain.Core.Models
{
    public class Block : BlockProof, IEquatable<Block>
    {

        public DateTime TimeStamp { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
        public string PreviousHash { get; set; }
        public long? CurrentIndex { get; set; }

        public bool Equals([AllowNull] Block other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Index == other.Index
               && Proof == other.Proof
               && TimeStamp == other.TimeStamp
               && PreviousHash == other.PreviousHash
               && CurrentIndex == other.CurrentIndex
               && Transactions.SequenceEqual(other.Transactions);
        }
    }
}
