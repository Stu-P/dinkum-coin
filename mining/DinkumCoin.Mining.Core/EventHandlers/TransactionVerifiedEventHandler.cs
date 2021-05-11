using System;
using System.Threading.Tasks;
using DinkumCoin.Domain.Events;
using DinkumCoin.Domain.EventHandlers;
using DinkumCoin.Mining.Core.Repositories;
using Microsoft.Extensions.Logging;

namespace DinkumCoin.Mining.Core.EventHandlers
{
    public class TransactionVerifiedEventHandler : IIntegrationEventHandler<TransactionVerifiedEvent>
    {
        private readonly IMiningRepository _miningRepository;
        private readonly ILogger<TransactionVerifiedEventHandler> _logger;
        // private readonly IMiningRepository
        public TransactionVerifiedEventHandler(IMiningRepository miningRepository, ILogger<TransactionVerifiedEventHandler> logger)
        {
            _miningRepository = miningRepository ?? throw new ArgumentNullException(nameof(miningRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(TransactionVerifiedEvent txnEvent)
        {

            _logger.LogInformation("Processing TransactionVerifiedEvent {@event}", txnEvent);
            try
            {
                await _miningRepository.UpdateTransactionToVerified(txnEvent.TransactionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured while processing TransactionVerifiedEvent");
            }
        }
    }
}