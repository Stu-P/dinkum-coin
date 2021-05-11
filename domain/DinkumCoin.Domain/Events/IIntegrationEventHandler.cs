using System.Threading.Tasks;
using DinkumCoin.Domain.Events;

namespace DinkumCoin.Domain.EventHandlers
{
    public interface IIntegrationEventHandler<in TEvent> where TEvent : IntegrationEvent
    {
        Task Handle(TEvent @event);
    }
}
