using System;
using System.Collections.Generic;
using DinkumCoin.Domain.Models;

namespace DinkumCoin.Blockchain.Core.Models
{
    public class SolutionAttempt
    {
        public Guid MinerId { get; set; }
        public long Proof { get; set; }
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
