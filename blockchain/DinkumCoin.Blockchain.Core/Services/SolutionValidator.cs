using System;
using Microsoft.Extensions.Options;

namespace DinkumCoin.Blockchain.Core.Services
{
    public class SolutionValidator : ISolutionValidator
    {
        private readonly IHashService _hashService;
        private readonly string _proofOfWorkSolution;


        public SolutionValidator(IHashService hashService, IOptions<SolutionValidatorSettings> settings)
        {
            _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
            _proofOfWorkSolution = settings?.Value?.ProofOfWorkSolution ?? throw new ArgumentNullException(nameof(settings));
        }

        public bool CheckProofOfWork(long previousProof, long attemptedProof)
        {
            string attempt = $"{previousProof}{attemptedProof}";
            string attemptAsHex = _hashService.ComputeHashHexString(attempt);

            if (attemptAsHex.Length < _proofOfWorkSolution.Length)
            {
                return false;
            }

            var lastNBytes = attemptAsHex.Substring(attemptAsHex.Length - _proofOfWorkSolution.Length, _proofOfWorkSolution.Length);
            return _proofOfWorkSolution == lastNBytes;
        }
    }
}
