using System.Threading;
using System.Threading.Tasks;

namespace DinkumCoin.Mining.Core.Services
{
    public interface IMiningService
    {
        Task StartAsync(CancellationToken cancellationToken = default);
    }
}
