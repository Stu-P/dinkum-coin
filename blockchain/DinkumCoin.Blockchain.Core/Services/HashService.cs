using System;
using System.Security.Cryptography;
using System.Text;

namespace DinkumCoin.Blockchain.Core.Services
{
    public class HashService : IHashService
    {
        public byte[] ComputeHashBytes(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                return bytes;
            }
        }

        public string ComputeHashHexString(string input)
        {
            var hex = BitConverter.ToString(ComputeHashBytes(input));
            return hex.Replace("-", "");
        }

        public bool ValidateHash(string testInput, string hash)
        {
            string testHash = ComputeHashHexString(testInput);
            testHash = testHash.Replace("-", "");

            return (string.Compare(testHash, hash, StringComparison.OrdinalIgnoreCase) == 0);
        }
    }
}
