using System;
using DinkumCoin.Domain.Models;
using DinkumCoin.Mining.Core.Models;

namespace DinkumCoin.Mining.Core.Mappers
{
    public static class TransactionMapper
    {
        public static Transaction From(MineableTransaction initial)
        {
            return new Transaction
            {
                Amount = initial.Amount,
                Id = initial.Id,
                Recipient = initial.Recipient,
                Sender = initial.Sender,
                TimeStamp = initial.TimeStamp
            };
        }

        public static MineableTransaction From(Transaction initial)
        {
            return new MineableTransaction
            {
                Amount = initial.Amount,
                Id = initial.Id,
                Recipient = initial.Recipient,
                Sender = initial.Sender,
                TimeStamp = initial.TimeStamp
            };
        }
    }
}
