using System.Collections.Generic;
using System.Threading.Tasks;
using DinkumCoin.Blockchain.Core.Models;

namespace DinkumCoin.Blockchain.Core.Repositories
{
    public interface IBlockchainRepository
    {
        Task<bool> AddBlock(Block newBlock);

        Task<Block> GetLastBlock();

        Task<IEnumerable<Block>> GetEntireChain();

        Task InitialiseChain();
    }
}
