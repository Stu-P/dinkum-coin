namespace DinkumCoin.Blockchain.Core.Services
{
    public interface ISolutionValidator
    {
        public bool CheckProofOfWork(long previousProof, long attemptedProof);

    }
}
