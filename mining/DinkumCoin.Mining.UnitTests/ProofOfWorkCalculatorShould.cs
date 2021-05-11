using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DinkumCoin.Mining.Core.Services;
using DinkumCoin.Mining.UnitTests.Framework;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace DinkumCoin.Mining.UnitTests
{
    public class ProofOfWorkCalculatorShould
    {
        public ITestOutputHelper _output;

        public ProofOfWorkCalculatorShould(ITestOutputHelper output) => _output = output;

        [Fact]
        public void CalulateCorrectProof()
        {

            var testLogger = new XUnitLogger<ProofOfWorkCalculator>(_output);
            var miner = new ProofOfWorkCalculator(testLogger);

            var timer = new Stopwatch();
            timer.Start();

            testLogger.Log(LogLevel.Information, "Starting calculation of proof of work");
            var result = miner.Calculate(15240);

            testLogger.Log(LogLevel.Information, $"Calculated answer of {result} after {timer.ElapsedMilliseconds} ms");
        }

        [Fact]
        public void ThrowExceptionOnCancelationRequest()
        {
            var testLogger = new XUnitLogger<ProofOfWorkCalculator>(_output);
            var miner = new ProofOfWorkCalculator(testLogger);

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            tokenSource.CancelAfter(50);

            var ex = Assert.ThrowsAnyAsync<TaskCanceledException>(async () =>
            {
                await Task.Run(() => miner.Calculate(15240, token), token);
            });
        }

        [Fact]
        public void ThrowExceptionIfCalculateCalledRepeatedly()
        {
            var testLogger = new XUnitLogger<ProofOfWorkCalculator>(_output);
            var miner = new ProofOfWorkCalculator(testLogger);

            var tasks = new List<Task>();
            var ex = Assert.ThrowsAnyAsync<TaskCanceledException>(async () =>
            {
                Task one = Task.Run(() => miner.Calculate(15240));
                Task two = Task.Run(() => miner.Calculate(15240));

                await Task.WhenAll(new Task[] { one, two });
            });
        }
    }
}
