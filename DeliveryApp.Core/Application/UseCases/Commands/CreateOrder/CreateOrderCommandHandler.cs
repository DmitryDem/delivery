using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, UnitResult<Error>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly IGeoClient geoClient;
        private readonly IUnitOfWork unitOfWork;

        public CreateOrderCommandHandler(
            IOrderRepository orderRepository,
            IGeoClient geoClient,
            IUnitOfWork unitOfWork)
        {
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            this.geoClient = geoClient;
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<UnitResult<Error>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var location = await this.geoClient.GetLocation(request.Street, cancellationToken);

            if (location.IsFailure)
            {
                return UnitResult.Failure(location.Error);
            }

            var order = Order.Create(request.BasketId, location.Value);

            if (!order.IsSuccess)
            {
                return UnitResult.Failure(order.Error);
            }

            await this.orderRepository.AddAsync(order.Value);
            await this.unitOfWork.SaveChangesAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }
    }
}
