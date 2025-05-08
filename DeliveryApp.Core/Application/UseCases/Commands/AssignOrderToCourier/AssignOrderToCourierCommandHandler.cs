using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.AssignOrderToCourier
{
    public class AssignOrderToCourierCommandHandler : IRequestHandler<AssignOrderToCourierCommand, bool>
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

        public async Task<bool> Handle(AssignOrderToCourierCommand request, CancellationToken cancellationToken)
        {
            // Получаем неназначенный заказ
            var order = await this.orderRepository.GetFirstInCreatedStatusAsync();

            if (order == null)
            {
                return false;
            }

            // Получаем свободных курьеров
            var couriers = this.courierRepository
                .GetAllInFreeStatus()
                .ToList();

            if (couriers.Count == 0)
            {
                return false;
            }

            // Распределяем заказ курьеру с наименьшим временем доставки
            var courier = this.dispatchService.Dispatch(order, couriers);

            if (courier.IsFailure)
            {
                return false;
            }

            this.orderRepository.Update(order);
            this.courierRepository.Update(courier.Value);

            return await this.unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
