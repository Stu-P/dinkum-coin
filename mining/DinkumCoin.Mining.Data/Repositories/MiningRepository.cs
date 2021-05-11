using System;
using System.Threading.Tasks;
using System.Linq;

using DinkumCoin.Mining.Core.Models;
using DinkumCoin.Mining.Core.Repositories;
using DinkumCoin.Mining.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DinkumCoin.Mining.Data.Repositories
{
    public class MiningRepository : IMiningRepository
    {
        private readonly MiningDbContext _miningDb;
        public MiningRepository(MiningDbContext miningDb)
        {
            _miningDb = miningDb ?? throw new ArgumentNullException(nameof(miningDb));

        }

        public async Task<IEnumerable<MineableTransaction>> GetUnverifiedTransactions()
        {
            return await _miningDb.Transactions.Where(x => !x.IsVerified).ToListAsync();
        }

        public async Task AddTransaction(MineableTransaction txn)
        {
            _miningDb.Transactions.Add(txn);
            await _miningDb.SaveChangesAsync();
        }

        public async Task UpdateTransactionsToVerified()
        {
            var transactions = await _miningDb.Transactions.Where(x => x.IsVerified == false).ToListAsync();
            foreach (var txn in transactions)
            {
                txn.IsVerified = true;

            }
            await _miningDb.SaveChangesAsync();
        }

        public async Task<MiningTask> GetLastMiningTask()
        {
            return await _miningDb.MiningTasks.AsNoTracking().OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        }

        public async Task AddMiningTask(MiningTask task)
        {
            _miningDb.MiningTasks.Add(task);
            await _miningDb.SaveChangesAsync();
        }

        public async Task UpdateMiningTask(MiningTask task)
        {
            _miningDb.Entry(task).State = EntityState.Modified;
            await _miningDb.SaveChangesAsync();
        }


    }
}
