using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder
{
    public class CreateOrderCommand : IRequest<bool>
    {
        public CreateOrderCommand(Guid basketId, string street)
        {
            if (basketId == Guid.Empty) throw new ArgumentNullException(nameof(basketId));
            if (string.IsNullOrWhiteSpace(street)) throw new ArgumentNullException(nameof(street));
            BasketId = basketId;
            Street = street;
        }

        public Guid BasketId { get; }

        public string Street { get; }
    }
}
