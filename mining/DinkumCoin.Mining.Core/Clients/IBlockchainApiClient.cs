using System.Threading.Tasks;
using DinkumCoin.Mining.Core.Models;

namespace DinkumCoin.Mining.Core.Clients
{
    public interface IBlockchainApiClient
    {
        Task<BlockProof> GetLastBlock();

        Task SubmitSolution(SolutionAttempt solution);
    }
}
