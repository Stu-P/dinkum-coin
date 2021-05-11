using System;
namespace DinkumCoin.Domain.Events
{
    public class TransactionVerifiedEvent : IntegrationEvent
    {
        public Guid TransactionId { get; private set; }
        public Guid VerifiedBy { get; private set; }

        public TransactionVerifiedEvent(Guid transactionId, Guid verifiedBy) : base("Transaction_Verified")
        {
            TransactionId = transactionId;
            VerifiedBy = verifiedBy;
        }
    }
}
