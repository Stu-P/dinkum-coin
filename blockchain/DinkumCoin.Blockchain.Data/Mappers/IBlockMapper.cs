using DinkumCoin.Blockchain.Core.Models;
using DinkumCoin.Blockchain.Data.Documents;

namespace DinkumCoin.Blockchain.Data.Mappers
{
    public interface IBlockMapper
    {
        Block From(BlockDocument block);
        BlockDocument From(Block block);
    }
}