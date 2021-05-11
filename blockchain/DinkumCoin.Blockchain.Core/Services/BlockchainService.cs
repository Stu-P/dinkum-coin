using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DinkumCoin.Blockchain.Core.Models;
using DinkumCoin.Blockchain.Core.Repositories;
using DinkumCoin.Domain.Events;
using DinkumCoin.Domain.Models;
using DinkumCoin.Domain.Services;
using DinkumCoin.Infrastructure.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DinkumCoin.Blockchain.Core.Services
{
    public class BlockchainService : IBlockchainService
    {
        private readonly ILogger<BlockchainService> _logger;
        private readonly IOptions<BlockchainServiceSettings> _settings;
        private readonly IBlockchainRepository _repo;
        private readonly IEventPublisher _eventPub;
        private readonly ISolutionValidator _validator;
        private readonly IHashService _hashService;
        private readonly IDateTimeGenerator _timeGenerator;
        private readonly IGuidGenerator _guidGenerator;

        public BlockchainService(
            ILogger<BlockchainService> logger,
            IOptions<BlockchainServiceSettings> settings,
            IBlockchainRepository repo,
            IEventPublisher eventPub,
            ISolutionValidator validator,
            IHashService hashService,
            IDateTimeGenerator timeGenerator,
            IGuidGenerator guidGenerator
            )
        {
            _eventPub = eventPub ?? throw new ArgumentNullException(nameof(eventPub));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _hashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
            _timeGenerator = timeGenerator ?? throw new ArgumentNullException(nameof(_timeGenerator));
            _guidGenerator = guidGenerator ?? throw new ArgumentNullException(nameof(guidGenerator));
        }

        public async Task<BlockProof> GetLastBlockProof() => await _repo.GetLastBlock();


        public async Task<IEnumerable<Block>> GetEntireChain()
        {
            return await _repo.GetEntireChain();
        }

        public async Task<bool> SolveCurrentBlock(SolutionAttempt solution)
        {
            _logger.LogInformation("Attempting to solve block with attempt {@solution}", solution);
            try
            {
                var lastBlock = await _repo.GetLastBlock();

                // validate provided solution
                bool success = _validator.CheckProofOfWork(lastBlock.Proof, solution.Proof);

                if (!success)
                {
                    _logger.LogWarning("An invalid solution was provided by miner {MinerId}:{solution}", solution.MinerId, solution);
                    return false;
                }

                // hash previous block
                var lastBlockHash = _hashService.ComputeHashHexString(JsonConvert.SerializeObject(lastBlock));

                // Create a new block from solution
                var newBlock = new Block()
                {
                    Index = ++lastBlock.Index,
                    Proof = solution.Proof,
                    TimeStamp = _timeGenerator.GenerateUTC(),
                    Transactions = solution.Transactions,
                    PreviousHash = lastBlockHash
                };

                success = await _repo.AddBlock(newBlock);
                if (!success)
                {
                    _logger.LogError($"Failed to save new block solution");
                    return false;
                }

                // Gift miner a txn
                var minerReward = new Transaction
                {
                    Id = _guidGenerator.Generate(),
                    Amount = _settings.Value.MiningReward,
                    TimeStamp = _timeGenerator.GenerateUTC(),
                    Recipient = solution.MinerId,
                    Sender = _settings.Value.NodeId
                };

                // TODO Post verified transactions 
                // await _repo.AddTransaction(minerReward);

                await _eventPub.Publish(new BlockCreatedEvent(newBlock.Index, _settings.Value.NodeId), _settings.Value.BlockchainTopic);

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"A failure occured while solving current block, see exception info for details");
                return false;
            }
        }

        public async Task<bool> ValidateChain()
        {
            var chain = (await _repo.GetEntireChain()).ToList();
            bool chainValid = true;

            for (int i = chain.Count - 1; i > 0; i--)
            {
                var expectedHash = chain[i].PreviousHash;
                var actualHash = _hashService.ComputeHashHexString(JsonConvert.SerializeObject(chain[i--]));

                chainValid = chainValid && (expectedHash == actualHash);
            }

            return chainValid;
        }
    }
}
