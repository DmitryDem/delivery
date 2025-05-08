using System;
using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Application
{
    public class MoveCouriersCommandShould
    {
        private readonly IOrderRepository orderRepository;
        private readonly ICourierRepository courierRepository;
        private readonly IUnitOfWork unitOfWork;

        public MoveCouriersCommandShould()
        {
            this.orderRepository = Substitute.For<IOrderRepository>();
            this.courierRepository = Substitute.For<ICourierRepository>();
            this.unitOfWork = Substitute.For<IUnitOfWork>();
        }

        [Fact]
        public async Task ShouldCorrectCompleteOrder()
        {
            //Arrange
            this.unitOfWork.SaveChangesAsync()
                .Returns(Task.FromResult(true));

            var courierLocation = Location.Create(4, 6)
                .Value;
            var orderLocation = Location.Create(6, 5)
                .Value;
            var courier = Courier.Create("Alex", "Car", 3, courierLocation)
                .Value;
            var order = Order.Create(Guid.NewGuid(), orderLocation)
                .Value;
            order.Assignee(courier);
            courier.SetBusy();

            this.orderRepository.GetAllInAssignStatus()
                .Returns([order]);
            this.courierRepository.GetAsync(Arg.Any<Guid>())
                .Returns(Task.FromResult(courier));

            var command = new MoveCouriersCommand();
            var handler = new MoveCouriersCommandHandler(this.orderRepository, this.courierRepository, this.unitOfWork);

            //Act
            var result = await handler.Handle(command, new CancellationToken());

            //Assert
            result.Should()
                .BeTrue();
            this.orderRepository.Received(1)
                .Update(
                    Arg.Is<Order>(
                        x => x.Id == order.Id &&
                             x.Status == OrderStatus.Completed()));
            this.courierRepository.Received(1)
                .Update(
                    Arg.Is<Courier>(
                        x => x.Id == courier.Id &&
                             x.Status == CourierStatus.Free() &&
                             x.Location == orderLocation));
        }

        [Fact]
        public async Task ShouldCorrectMoveCourier()
        {
            //Arrange
            this.unitOfWork.SaveChangesAsync()
                .Returns(Task.FromResult(true));

            var courierLocation = Location.Create(4, 6)
                .Value;
            var orderLocation = Location.Create(8, 4)
                .Value;
            var courier = Courier.Create("Alex", "Car", 3, courierLocation)
                .Value;
            var order = Order.Create(Guid.NewGuid(), orderLocation)
                .Value;
            order.Assignee(courier);
            courier.SetBusy();

            this.orderRepository.GetAllInAssignStatus()
                .Returns([order]);
            this.courierRepository.GetAsync(Arg.Any<Guid>())
                .Returns(Task.FromResult(courier));

            var command = new MoveCouriersCommand();
            var handler = new MoveCouriersCommandHandler(this.orderRepository, this.courierRepository, this.unitOfWork);

            //Act
            var result = await handler.Handle(command, new CancellationToken());

            //Assert
            result.Should()
                .BeTrue();
            this.orderRepository.Received(0)
                .Update(Arg.Any<Order>());
            this.courierRepository.Received(1)
                .Update(
                    Arg.Is<Courier>(
                        x => x.Id == courier.Id &&
                             x.Status == CourierStatus.Busy() &&
                             x.Location == Location.Create(7, 6).Value));
        }
    }
}
