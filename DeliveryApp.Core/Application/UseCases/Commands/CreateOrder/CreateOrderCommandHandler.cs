using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, bool>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IUnitOfWork unitOfWork;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository, 
            IUnitOfWork unitOfWork)
        {
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<bool> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // тут будет определение Location по Street. Пока генерируем рандомную локацию
            var location = Location.CreateRandom();
            var order = Order.Create(request.BasketId, location.Value);

            if (!order.IsSuccess)
            {
                return false;
            }

            await this.orderRepository.AddAsync(order.Value);
            return await this.unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
