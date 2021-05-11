using System.Threading;

namespace DinkumCoin.Mining.Core.Services
{
    public interface IProofOfWorkCalculator
    {
        public long Calculate(long lastProof, CancellationToken cancelationToken);
    }
}
