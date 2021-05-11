using System.Threading;
using System.Threading.Tasks;

namespace DinkumCoin.Infrastructure.Messaging
{
    public interface IEventConsumer
    {
        Task Start(string streamName, CancellationToken token = default(CancellationToken));
    }
}
