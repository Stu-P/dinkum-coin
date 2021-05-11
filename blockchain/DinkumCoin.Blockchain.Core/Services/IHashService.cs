namespace DinkumCoin.Blockchain.Core.Services
{
    public interface IHashService
    {
        public byte[] ComputeHashBytes(string input);
        public string ComputeHashHexString(string input);
        public bool ValidateHash(string testInput, string hash);
    }
}
