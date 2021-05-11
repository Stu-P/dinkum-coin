using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DinkumCoin.Mining.Core.Clients;
using DinkumCoin.Mining.Core.Mappers;
using DinkumCoin.Mining.Core.Models;
using DinkumCoin.Mining.Core.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DinkumCoin.Mining.Core.Services
{
    public class MiningService : IMiningService
    {
        private const int PollIntervalMS = 15000;

        private readonly IProofOfWorkCalculator _calculator;
        private readonly IBlockchainApiClient _apiClient;
        private readonly IMiningRepository _miningRepository;
        private readonly ICancelTokenProvider _cancelTokenProvider;
        private readonly IOptions<MiningServiceSettings> _settings;
        private readonly ILogger<MiningService> _logger;



        public MiningService(
            IProofOfWorkCalculator calculator,
            IBlockchainApiClient apiClient,
            IMiningRepository miningRepository,
            ICancelTokenProvider cancelTokenProvider,
            IOptions<MiningServiceSettings> settings,
            ILogger<MiningService> logger)
        {
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
            _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
            _miningRepository = miningRepository ?? throw new ArgumentNullException(nameof(miningRepository));
            _cancelTokenProvider = cancelTokenProvider ?? throw new ArgumentNullException(nameof(cancelTokenProvider));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task StartAsync(CancellationToken stoppingToken = default)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentTask = await _miningRepository.GetLastMiningTask();

                if (currentTask == null || currentTask.Status != MiningTaskStatus.Active)
                {
                    _logger.LogInformation("Mining service checking for any mineable transactions");
                    var unverified = await _miningRepository.GetUnverifiedTransactions();

                    if (unverified.Any())
                    {
                        _logger.LogInformation("Found {count} mineable transactions, will begin mining for solution", unverified.Count());

                        var currentProof = await _apiClient.GetLastBlock();

                        using (CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken, _cancelTokenProvider.New()))
                        {
                            var miningTask = new MiningTask
                            {
                                Status = MiningTaskStatus.Active,
                                StartTime = DateTime.UtcNow,
                                BlockId = currentProof.Index
                            };

                            await _miningRepository.AddMiningTask(miningTask);

                            long calculatedSolution = -1;
                            var timer = new Stopwatch();
                            try
                            {
                                timer.Start();
                                _logger.LogInformation("Starting solution attempt for block {id}", currentProof.Index);

                                await Task.Run(() => calculatedSolution = _calculator.Calculate(currentProof.Proof, linkedCts.Token));

                                _logger.LogInformation("Solution calculated for block {id} in {durationMS} ms", currentProof.Index, timer.ElapsedMilliseconds);
                                await HandleMiningSolutionFound(miningTask, calculatedSolution);
                            }
                            catch (OperationCanceledException)
                            {
                                _logger.LogInformation("Mining solution for block {id} cancelled after {durationMS} ms", currentProof.Index, timer.ElapsedMilliseconds);
                                await HandleMiningCanceled(miningTask);
                            }
                        }
                    }
                    _logger.LogInformation("No mineable transactions found, will check again in {interval} ms", PollIntervalMS);
                }
                else
                {
                    _logger.LogInformation("existing mining task in progress, will check again in {interval} ms", PollIntervalMS);
                }
                await Task.Delay(PollIntervalMS);
            }
        }

        private async Task HandleMiningSolutionFound(MiningTask miningTask, long calculatedSolution)
        {
            // retrieve up to date list of unverified transactions
            var unverified = await _miningRepository.GetUnverifiedTransactions();

            var solutionAttempt = new SolutionAttempt
            {
                MinerId = new Guid(_settings?.Value?.MinerId),
                Proof = calculatedSolution,
                Transactions = unverified.Select(i => TransactionMapper.From(i)).ToList()
            };

            miningTask.EndTime = DateTime.UtcNow;
            miningTask.Status = MiningTaskStatus.Complete;
            miningTask.Solution = calculatedSolution.ToString();

            List<Task> tasks = new List<Task>
            {
                _miningRepository.UpdateMiningTask(miningTask),
                _miningRepository.UpdateTransactionsToVerified(),
                _apiClient.SubmitSolution(solutionAttempt)
            };

            await Task.WhenAll(tasks);

            _logger.LogInformation("Solution submitted {solution}", solutionAttempt);

        }

        private async Task HandleMiningCanceled(MiningTask miningTask)
        {

            miningTask.EndTime = DateTime.UtcNow;
            miningTask.Status = MiningTaskStatus.Cancelled;
            await _miningRepository.UpdateMiningTask(miningTask);
        }

    }
}
