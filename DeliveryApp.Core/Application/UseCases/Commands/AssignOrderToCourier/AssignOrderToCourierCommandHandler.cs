using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrderToCourier
{
    public class AssignOrderToCourierCommandHandler : IRequestHandler<AssignOrderToCourierCommand, UnitResult<Error>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly ICourierRepository courierRepository;
        private readonly IDispatchService dispatchService;
        private readonly IUnitOfWork unitOfWork;

        public AssignOrderToCourierCommandHandler(
            IOrderRepository orderRepository,
            ICourierRepository courierRepository,
            IDispatchService dispatchService,
            IUnitOfWork unitOfWork)
        {
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            this.courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
            this.dispatchService = dispatchService ?? throw new ArgumentNullException(nameof(dispatchService));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<UnitResult<Error>> Handle(AssignOrderToCourierCommand request, CancellationToken cancellationToken)
        {
            // Получаем неназначенный заказ
            var order = await this.orderRepository.GetFirstInCreatedStatusAsync();

            if (order == null)
            {
                return Errors.OrderInCreatedStatusNotFound();
            }

            // Получаем свободных курьеров
            var couriers = await this.courierRepository
                .GetAllInFreeStatus();

            if (couriers.Count == 0)
            {
                return Errors.CourierInFreeStatusNotFound();
            }

            // Распределяем заказ курьеру с наименьшим временем доставки
            var courier = this.dispatchService.Dispatch(order, couriers);

            if (courier.IsFailure)
            {
                return Errors.DispatchOrderFailure();
            }

            this.orderRepository.Update(order);
            this.courierRepository.Update(courier.Value);
            
            await this.unitOfWork.SaveChangesAsync(cancellationToken);

            return UnitResult.Success<Error>();
        }

        public static class Errors
        {
            public static Error OrderInCreatedStatusNotFound()
            {
                return new Error($"{nameof(Order).ToLowerInvariant()}.in.created.status.not.found", "Order in created status not found");
            }

            public static Error CourierInFreeStatusNotFound()
            {
                return new Error($"{nameof(Courier).ToLowerInvariant()}.in.free.status.not.found", "Courier in free status not found");
            }

            public static Error DispatchOrderFailure()
            {
                return new Error($"{nameof(Order).ToLowerInvariant()}.dispatch.failure", "Order dispatch failure");
            }
        }
    }
}
