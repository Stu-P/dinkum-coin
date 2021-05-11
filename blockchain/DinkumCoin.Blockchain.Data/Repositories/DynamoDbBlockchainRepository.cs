using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using DinkumCoin.Blockchain.Core.Exceptions;
using DinkumCoin.Blockchain.Core.Models;
using DinkumCoin.Blockchain.Core.Repositories;
using DinkumCoin.Blockchain.Data.Clients;
using DinkumCoin.Blockchain.Data.Documents;
using DinkumCoin.Blockchain.Data.Mappers;
using Microsoft.Extensions.Logging;

namespace DinkumCoin.Blockchain.Data.Repositories
{
    public class DynamoDbBlockchainRepository : IBlockchainRepository
    {
        private readonly ILogger<DynamoDbBlockchainRepository> _logger;
        private readonly IDynamoDBContext _dbContext;
        private readonly IBlockMapper _blockMapper;
        private readonly IDynamoDbClient _bcClient;

        public DynamoDbBlockchainRepository(ILogger<DynamoDbBlockchainRepository> logger, IDynamoDbClient bcClient, IDynamoDBContext dbContext, IBlockMapper blockMapper)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _bcClient = bcClient ?? throw new ArgumentNullException(nameof(bcClient));
            _blockMapper = blockMapper ?? throw new ArgumentNullException(nameof(blockMapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> AddBlock(Block newBlock)
        {
            var header = await GetHeader();
            header.CurrentIndex = newBlock.Index;

            var allTasks = new List<Task>();
            allTasks.Add(_bcClient.Upsert(_dbContext.ToDocument(header)));
            allTasks.Add(_bcClient.Upsert(_dbContext.ToDocument(_blockMapper.From(newBlock))));
            Task.WaitAll(allTasks.ToArray());
            return true;
        }

        public async Task<IEnumerable<Block>> GetEntireChain()
        {
            var chain = await _bcClient.GetAll();
            var mappedChain = chain.Select(item =>
            _blockMapper.From(_dbContext.FromDocument<BlockDocument>(item)));
            return mappedChain.OrderByDescending(i => i.Index);
        }

        public async Task<Block> GetLastBlock()
        {
            var header = await GetHeader();

            var result = await _bcClient.GetItem(header.CurrentIndex.Value);
            if (result == null)
            {
                _logger.LogError("Cannot find block indicated by current {index}", header.CurrentIndex.Value);
                throw new NotFoundException($"Cannot find block indicated by current index: {header.CurrentIndex.Value}");
            }
            return _blockMapper.From(_dbContext.FromDocument<BlockDocument>(result));
        }

        public async Task InitialiseChain()
        {
            var result = await _bcClient.GetItem(0);
            if (result == null)
            {
                _logger.LogInformation("Header record does not exist, initialising from Seed document");
                var seedDoc = _blockMapper.From(SeedBlock);
                await _bcClient.Upsert(_dbContext.ToDocument(seedDoc));
            }
        }

        private async Task<BlockDocument> GetHeader()
        {
            var result = await _bcClient.GetItem(0);
            return _dbContext.FromDocument<BlockDocument>(result);
        }



        private Block SeedBlock =>
            new Block
            {
                Index = 0,
                CurrentIndex = 0,
                PreviousHash = "1",
                Proof = 1000,
                TimeStamp = DateTime.UtcNow
            };
    }
}
