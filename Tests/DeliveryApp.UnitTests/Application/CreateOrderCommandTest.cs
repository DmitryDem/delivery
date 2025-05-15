using System;
using System.Threading;
using System.Threading.Tasks;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using FluentAssertions;
using NSubstitute;
using Primitives;
using Xunit;

namespace DeliveryApp.UnitTests.Application
{
    public class CreateOrderCommandShould
    {
        private readonly IOrderRepository orderRepository;
        private readonly IGeoClient geoClient;
        private readonly IUnitOfWork unitOfWork;

        public CreateOrderCommandShould()
        {
            this.orderRepository = Substitute.For<IOrderRepository>();
            this.geoClient = Substitute.For<IGeoClient>();
            this.unitOfWork = Substitute.For<IUnitOfWork>();
        }

        [Fact]
        public async Task CanAddNewOrder()
        {
            //Arrange
            this.unitOfWork.SaveChangesAsync()
                .Returns(Task.FromResult(true));

            this.geoClient.GetLocation(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Location.CreateRandom().Value);

            var command = new CreateOrderCommand(Guid.NewGuid(), "Some street");
            var handler = new CreateOrderCommandHandler(this.orderRepository, this.geoClient, this.unitOfWork);

            //Act
            var result = await handler.Handle(command, new CancellationToken());

            //Assert
            result.IsSuccess.Should().BeTrue();
            this.orderRepository.Received(1).AddAsync(Arg.Is<Order>(
                x => x.Id == command.BasketId &&
                     x.Status == OrderStatus.Created() &&
                     x.CourierId == null &&
                     x.Location != null));
        }

        [Fact]
        public async Task ReturnErrorIfGetIncorrectLocation()
        {
            //Arrange
            this.geoClient.GetLocation(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Location.Create(100, 100).Error);

            var command = new CreateOrderCommand(Guid.NewGuid(), "Some street");
            var handler = new CreateOrderCommandHandler(this.orderRepository, this.geoClient, this.unitOfWork);

            //Act
            var result = await handler.Handle(command, new CancellationToken());

            //Assert
            result.IsFailure.Should().BeTrue();
        }
    }
}
