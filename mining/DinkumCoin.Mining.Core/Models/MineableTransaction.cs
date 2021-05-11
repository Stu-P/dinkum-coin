using System;
namespace DinkumCoin.Mining.Core.Models
{
    public class MineableTransaction
    {
        public Guid Id { get; set; }
        public bool IsVerified { get; set; } = false;
        public Guid Sender { get; set; }
        public Guid Recipient { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal Amount { get; set; }
    }
}
