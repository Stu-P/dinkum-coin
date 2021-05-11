using System.Collections.Generic;
using System.Threading.Tasks;
using DinkumCoin.Blockchain.Core.Models;

namespace DinkumCoin.Blockchain.Core.Services
{
    public interface IBlockchainService
    {
        Task<bool> ValidateChain();

        Task<BlockProof> GetLastBlockProof();

        Task<IEnumerable<Block>> GetEntireChain();

        Task<bool> SolveCurrentBlock(SolutionAttempt solution);
    }
}
