using System;
using System.Diagnostics.CodeAnalysis;

namespace DinkumCoin.Domain.Models
{
    public class Transaction : IEquatable<Transaction>
    {
        public Guid Id { get; set; }
        public Guid Sender { get; set; }
        public Guid Recipient { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal Amount { get; set; }

        public bool Equals([AllowNull] Transaction other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id
               && Sender == other.Sender
               && TimeStamp == other.TimeStamp
               && Recipient == other.Recipient
               && TimeStamp == other.TimeStamp
               && Amount == other.Amount;
        }
    }
}
