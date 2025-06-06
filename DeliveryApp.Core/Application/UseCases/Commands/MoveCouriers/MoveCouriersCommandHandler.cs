﻿using CSharpFunctionalExtensions;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers
{
    public class MoveCouriersCommandHandler : IRequestHandler<MoveCouriersCommand, UnitResult<Error>>
    {
        private readonly IOrderRepository orderRepository;
        private readonly ICourierRepository courierRepository;
        private readonly IUnitOfWork unitOfWork;

        public MoveCouriersCommandHandler(
            IOrderRepository orderRepository,
            ICourierRepository courierRepository,
            IUnitOfWork unitOfWork)
        {
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            this.courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<UnitResult<Error>> Handle(MoveCouriersCommand request, CancellationToken cancellationToken)
        {
            // Получаем все заказы назначенные на курьеров
            var orders = await orderRepository.GetAllInAssignStatus();

            foreach (var order in orders)
            {
                var courier = await courierRepository.GetAsync(order.CourierId!.Value);
                if (courier == null) continue;

                courier.Move(order.Location);

                // Проверяем, достиг ли курьер места назначения
                if (courier.Location == order.Location)
                {
                    // Если достиг, то завершаем заказ и освобождаем курьера
                    order.Complete();
                    courier.SetFree();

                    orderRepository.Update(order);
                }

                courierRepository.Update(courier);

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return UnitResult.Success<Error>();
        }
    }
}
