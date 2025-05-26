using Primitives;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents
{
    public record OrderCompletedDomainEvent(Guid OrderId) : DomainEvent;
}
