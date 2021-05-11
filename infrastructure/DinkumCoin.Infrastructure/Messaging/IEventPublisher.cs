using System.Threading.Tasks;
using DinkumCoin.Domain.Events;

namespace DinkumCoin.Infrastructure.Messaging
{
    public interface IEventPublisher
    {
        Task Publish(IntegrationEvent @event, string topic);
    }
}
