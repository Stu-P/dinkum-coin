using System;
using DinkumCoin.Blockchain.Core.Models;
using DinkumCoin.Blockchain.Data.Documents;

namespace DinkumCoin.Blockchain.Data.Mappers
{
    public class BlockMapper : IBlockMapper
    {
        public Block From(BlockDocument block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block), $"Argument cannot be null");
            }
            return new Block
            {
                Index = block.BlockId,
                CurrentIndex = block.CurrentIndex,
                TimeStamp = block.TimeStamp,
                Proof = block.Proof,
                PreviousHash = block.PreviousHash,
                Transactions = block.Transactions
            };
        }

        public BlockDocument From(Block block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block), $"Argument cannot be null");
            }
            return new BlockDocument
            {
                BlockId = block.Index,
                CurrentIndex = block.CurrentIndex,
                TimeStamp = block.TimeStamp,
                Proof = block.Proof,
                PreviousHash = block.PreviousHash,
                Transactions = block.Transactions
            };
        }
    }
}
