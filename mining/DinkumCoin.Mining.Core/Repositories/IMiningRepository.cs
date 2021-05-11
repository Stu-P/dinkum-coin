using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DinkumCoin.Mining.Core.Models;

namespace DinkumCoin.Mining.Core.Repositories
{
    public interface IMiningRepository
    {
        Task AddTransaction(MineableTransaction txn);
        Task<IEnumerable<MineableTransaction>> GetUnverifiedTransactions();
        Task UpdateTransactionsToVerified();
        Task<MiningTask> GetLastMiningTask();
        Task AddMiningTask(MiningTask task);
        Task UpdateMiningTask(MiningTask task);
    }
}
