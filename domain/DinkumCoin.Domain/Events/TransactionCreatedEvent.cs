using System;
using DinkumCoin.Domain.Models;

namespace DinkumCoin.Domain.Events
{
    public class TransactionCreatedEvent : IntegrationEvent
    {
        public TransactionCreatedEvent(Transaction newTransaction) : base("Transaction_Created")
        {
            NewTransaction = newTransaction;
        }
        public Transaction NewTransaction { get; set; }
    }
}
