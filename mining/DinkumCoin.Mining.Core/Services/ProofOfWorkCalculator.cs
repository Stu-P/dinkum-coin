using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace DinkumCoin.Mining.Core.Services
{
    public class ProofOfWorkCalculator : IProofOfWorkCalculator
    {
        private const string Answer = "000000";

        private bool _inProgress;
        private readonly ILogger<ProofOfWorkCalculator> _logger;

        public ProofOfWorkCalculator(ILogger<ProofOfWorkCalculator> logger)
        {
            _inProgress = false;
            _logger = logger;
        }

        public long Calculate(long lastProof, CancellationToken cancelationToken = default(CancellationToken))
        {
            if (cancelationToken.IsCancellationRequested)
            {
                cancelationToken.ThrowIfCancellationRequested();
            }
            if (_inProgress)
            {
                throw new InvalidOperationException($"Cannot call {nameof(Calculate)} while a previous calculation is in progress");
            }

            long attempt = 0;

            using (SHA256 sha256Hash = SHA256.Create())
            {
                var timer = new Stopwatch();
                do
                {
                    timer.Start();
                    if (cancelationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Proof of work calculation cancelled after {duration} ms", timer.ElapsedMilliseconds);
                        cancelationToken.ThrowIfCancellationRequested();
                    }

                    string guess = $"{lastProof}{attempt}";

                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(guess));
                    var hex = BitConverter.ToString(bytes).Replace("-", "");

                    var lastBytes = hex.Substring(hex.Length - Answer.Length, Answer.Length);

                    if (lastBytes == Answer)
                    {
                        _logger.LogInformation("Solution found with {guess} after {duration} ms", attempt, timer.ElapsedMilliseconds);
                        return attempt;
                    }
                    attempt++;

                } while (true);
            }
        }
    }
}
