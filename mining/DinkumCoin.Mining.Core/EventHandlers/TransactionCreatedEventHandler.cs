using System;
using System.Threading.Tasks;
using DinkumCoin.Domain.Events;
using DinkumCoin.Domain.EventHandlers;
using DinkumCoin.Mining.Core.Repositories;
using DinkumCoin.Mining.Core.Mappers;
using Microsoft.Extensions.Logging;

namespace DinkumCoin.Mining.Core.EventHandlers
{
    public class TransactionCreatedEventHandler : IIntegrationEventHandler<TransactionCreatedEvent>
    {
        private readonly IMiningRepository _miningRepository;
        private readonly ILogger<TransactionCreatedEventHandler> _logger;

        public TransactionCreatedEventHandler(IMiningRepository miningRepository, ILogger<TransactionCreatedEventHandler> logger)
        {
            _miningRepository = miningRepository ?? throw new ArgumentNullException(nameof(miningRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Handle(TransactionCreatedEvent txnEvent)
        {
            _logger.LogInformation("Processing TransactionCreatedEvent {@event}", txnEvent);

            try
            {
                await _miningRepository.AddTransaction(TransactionMapper.From(txnEvent.NewTransaction));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured while processing TransactionCreatedEvent");
            }
        }
    }
}
