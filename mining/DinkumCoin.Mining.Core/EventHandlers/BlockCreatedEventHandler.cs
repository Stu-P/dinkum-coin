using System;
using System.Threading.Tasks;
using DinkumCoin.Domain.EventHandlers;
using DinkumCoin.Domain.Events;
using DinkumCoin.Mining.Core.Models;
using DinkumCoin.Mining.Core.Repositories;
using DinkumCoin.Mining.Core.Services;
using Microsoft.Extensions.Logging;

namespace DinkumCoin.Mining.Core.EventHandlers
{
    public class BlockCreatedEventHandler : IIntegrationEventHandler<BlockCreatedEvent>
    {

        private readonly IMiningRepository _miningRepository;
        private readonly ICancelTokenProvider _cancelTokenProvider;
        private readonly ILogger<BlockCreatedEventHandler> _logger;

        public BlockCreatedEventHandler(
            IMiningRepository miningRepository,
            ICancelTokenProvider cancelTokenProvider,
            ILogger<BlockCreatedEventHandler> logger
            )
        {
            _miningRepository = miningRepository ?? throw new ArgumentNullException(nameof(miningRepository));
            _cancelTokenProvider = cancelTokenProvider ?? throw new ArgumentNullException(nameof(cancelTokenProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); ;
        }

        public async Task Handle(BlockCreatedEvent createdEvent)
        {
            _logger.LogInformation("Received BlockCreatedEvent {@event}, checking for any in progress mining tasks", createdEvent);

            var currentTask = await _miningRepository.GetLastMiningTask();
            if (currentTask == null || currentTask.Status != MiningTaskStatus.Active)
            {
                _logger.LogInformation("No mining tasks in progress, no action required");
                return;
            }

            _logger.LogInformation("Found mining task in progress, cancelling {task}", currentTask);
            currentTask.Status = MiningTaskStatus.Cancelled;
            await _miningRepository.UpdateMiningTask(currentTask);

            _cancelTokenProvider.CancelExisting();

            // get last mining task

            // Is mining in progress? if so, cancel
            // _cancelTokenProvider.CancelExisting();


            //var currentBlock = createdEvent?.Block;

            //if (currentBlock == null)
            //{
            //    _logger.LogError("Event is missing required data, {@event}", createdEvent);
            //    return;
            //}

            //if (IsDuplicateMessage(currentBlock.Index))
            //{
            //    _logger.LogInformation("Duplicate event for {blockId}, discarding", currentBlock.Index);
            //    return;
            //}

            //var newCancelToken = AbortExisting(currentBlock.Index);

            //long calculatedSolution = -1;
            //var timer = new Stopwatch();
            //try
            //{
            //    timer.Start();
            //    _logger.LogInformation("Starting solution attempt for block {id}", currentBlock.Index);


            //    await Task.Run(() => calculatedSolution = _calculator.Calculate(currentBlock.Proof, newCancelToken));

            //    _logger.LogInformation("Solution calculated for block {id} in {durationMS} ms", currentBlock.Index, timer.ElapsedMilliseconds);
            //    var solutionAttempt = new SolutionAttempt
            //    {
            //        MinerId = new Guid(Consts.MinerId),
            //        Proof = calculatedSolution
            //    };

            //    await _apiClient.SubmitSolution(solutionAttempt);
            //    _logger.LogInformation("Solution submitted {@solution}", solutionAttempt);
            //}
            //catch (TaskCanceledException)
            //{
            //    _logger.LogInformation("Mining solution for block {id} cancelled after {durationMS} ms", currentBlock.Index, timer.ElapsedMilliseconds);
            //}

        }

        //private bool IsDuplicateMessage(long index)
        //{
        //    var hasKey = _cache.TryGetValue(
        //        Consts.MineJobInProgressCacheKey,
        //        out long currentBlockId
        //        );

        //    if (hasKey && index == currentBlockId)
        //    {
        //        return true;
        //    }

        //    _cache.Set(Consts.MineJobInProgressCacheKey, index);

        //    return false;
        //}


        //private CancellationToken AbortExisting(long index)
        //{

        //    var hasKey = _cache.TryGetValue(
        //        Consts.MiningCancelTokenCacheKey,
        //        out CancellationTokenSource cts
        //        );

        //    if (hasKey)
        //    {
        //        _logger.LogInformation("Canceling any existing mining attempt");
        //        cts.Cancel();
        //    }
        //    var newCts = new CancellationTokenSource();
        //    _cache.Set(Consts.MiningCancelTokenCacheKey, newCts);

        //    return newCts.Token;
        //}
    }
}
