using System;
using System.Threading;
using DeliveryApp.Core.Domain.Services;
using DeliveryApp.Core.Ports;
using NSubstitute;
using Primitives;
using System.Threading.Tasks;
using DeliveryApp.Core.Application.UseCases.Commands.AssignOrderToCourier;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using Xunit;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using FluentAssertions;

namespace DeliveryApp.UnitTests.Application
{
    public class AssignOrderToCourierCommandShould
    {
        private readonly IOrderRepository orderRepository;
        private readonly ICourierRepository courierRepository;
        private readonly IDispatchService dispatchService;
        private readonly IUnitOfWork unitOfWork;

        public AssignOrderToCourierCommandShould()
        {
            this.orderRepository = Substitute.For<IOrderRepository>();
            this.courierRepository = Substitute.For<ICourierRepository>();
            this.dispatchService = new DispatchService();
            this.unitOfWork = Substitute.For<IUnitOfWork>();
        }

        [Fact]
        public async Task ShouldReturnErrorIfNoUnassignedOrder()
        {
            // Arrange
            this.unitOfWork.SaveChangesAsync()
                .Returns(Task.FromResult(true));

            var orderId = Guid.NewGuid();
            var order = Order.Create(orderId, Location.Create(1, 2).Value).Value;

            this.orderRepository.GetFirstInCreatedStatusAsync()
                .Returns(Task.FromResult((Order)null));

            var command = new AssignOrderToCourierCommand();
            var handler = new AssignOrderToCourierCommandHandler(this.orderRepository, this.courierRepository, this.dispatchService, this.unitOfWork);

            // Act
            var result = await handler.Handle(command, new CancellationToken());

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldReturnErrorIfNoAvailableCouriers()
        {
            // Arrange
            this.unitOfWork.SaveChangesAsync()
                .Returns(Task.FromResult(true));

            var orderId = Guid.NewGuid();
            var order = Order.Create(orderId, Location.Create(1, 2).Value).Value;

            this.orderRepository.GetFirstInCreatedStatusAsync()
                .Returns(Task.FromResult(order));

            var command = new AssignOrderToCourierCommand();
            var handler = new AssignOrderToCourierCommandHandler(this.orderRepository, this.courierRepository, this.dispatchService, this.unitOfWork);

            // Act
            var result = await handler.Handle(command, new CancellationToken());

            // Assert
            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ShouldAssignOnCourierWithLessDeliveryTime()
        {
            // Arrange
            this.unitOfWork.SaveChangesAsync()
                .Returns(Task.FromResult(true));

            var orderId = Guid.NewGuid();
            var order = Order.Create(orderId, Location.Create(7, 7).Value).Value;

            this.orderRepository.GetFirstInCreatedStatusAsync()
                .Returns(Task.FromResult(order));

            var courier1Location = Location.Create(9, 5).Value;
            var courier2Location = Location.Create(1, 8).Value;
            var courier3Location = Location.Create(3, 2).Value;

            var courier1 = Courier.Create("Alex", "Foot", 1, courier1Location).Value;
            var courier2 = Courier.Create("Bob", "Car", 3, courier2Location).Value;
            var courier3 = Courier.Create("Charlie", "Bike", 2, courier3Location).Value;

            this.courierRepository.GetAllInFreeStatus()
                .Returns(new[] { courier1, courier2, courier3 });

            var command = new AssignOrderToCourierCommand();
            var handler = new AssignOrderToCourierCommandHandler(this.orderRepository, this.courierRepository, this.dispatchService, this.unitOfWork);

            // Act
            var result = await handler.Handle(command, new CancellationToken());

            // Assert
            result.IsSuccess.Should().BeTrue();
            this.orderRepository.Received(1)
                .Update(
                    Arg.Is<Order>(
                        x => x.Id == order.Id &&
                             x.Status == OrderStatus.Assigned()));

            this.courierRepository.Received(1)
                .Update(
                    Arg.Is<Courier>(
                        x => x.Id == courier2.Id &&
                           x.Status == CourierStatus.Busy()));
        }
    }
}
