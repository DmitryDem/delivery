using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using MediatR;

namespace DeliveryApp.Core.Application.DomainEventHandlers
{
    public class OrderCompletedDomainEventHandler : INotificationHandler<OrderCompletedDomainEvent>
    {
        private readonly IMessageBusProducer producer;

        public OrderCompletedDomainEventHandler(IMessageBusProducer producer)
        {
            this.producer = producer ?? throw new ArgumentNullException(nameof(producer));
        }

        public Task Handle(OrderCompletedDomainEvent notification, CancellationToken cancellationToken)
        {
            return this.producer.Publish(notification, cancellationToken);
        }
    }
}
