using System;
using System.Collections.Generic;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.OrderAggregate
{
    public class OrderShould
    {
        public static IEnumerable<object[]> GetInvalidOrderData()
        {
            yield return [Guid.Empty, Location.Create(1, 1).Value];
            yield return [Guid.NewGuid(), null];
        }

        [Fact]
        public void BeCorrectWhenParamsIsCorrectOnCreated()
        {
            //Arrange
            var basketId = Guid.NewGuid();
            var location = Location.CreateRandom().Value;

            //Act
            var order = Order.Create(basketId, location);

            //Assert
            order.IsSuccess.Should().BeTrue();
            order.Value.Id.Should().Be(basketId);
            order.Value.Status.Should().Be(OrderStatus.Created());
            order.Value.Location.Should().NotBeNull();
            order.Value.Location.Should().Be(location);
            order.Value.CourierId.Should().BeNull();
        }

        [Theory]
        [MemberData(nameof(GetInvalidOrderData))]
        public void ReturnErrorWhenParamsIsIncorrectOnCreated(Guid basketId, Location location)
        {
            //Arrange

            //Act
            var order = Order.Create(basketId, location);

            //Assert
            order.IsSuccess.Should().BeFalse();
            order.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeAssignedToCourier()
        {
            //Arrange
            var basketId = Guid.NewGuid();
            var location = Location.CreateRandom().Value;
            var order = Order.Create(basketId, location);
            var courier = Courier.Create("Alex", "Car", 3, Location.CreateRandom().Value);

            //Act
            order.Value.Assignee(courier.Value);

            //Assert
            order.IsSuccess.Should().BeTrue();
            order.Value.CourierId.Should().NotBeNull();
            order.Value.CourierId.Should().Be(courier.Value.Id);
            order.Value.Status.Should().Be(OrderStatus.Assign());
        }

        [Fact]
        public void ReturnErrorIfAssignCourierParamsIsIncorrect()
        {
            //Arrange
            var order = Order.Create(Guid.NewGuid(),Location.CreateRandom().Value);

            //Act
            var result = order.Value.Assignee(null);

            //Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void ReturnErrorIfAssignCourierOnCompletedOrder()
        {
            //Arrange
            var order = Order.Create(Guid.NewGuid(), Location.CreateRandom().Value);
            var courier = Courier.Create("Alex", "Car", 3, Location.CreateRandom().Value);
            order.Value.Assignee(courier.Value);
            order.Value.Complete();

            //Act
            var result = order.Value.Assignee(courier.Value);

            //Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeComplete()
        {
            //Arrange
            var basketId = Guid.NewGuid();
            var location = Location.CreateRandom().Value;
            var order = Order.Create(basketId, location);
            var courier = Courier.Create("Alex", "Car", 3, Location.CreateRandom().Value);
            order.Value.Assignee(courier.Value);

            //Act
            order.Value.Complete();

            //Assert
            order.IsSuccess.Should().BeTrue();
            order.Value.Status.Should().Be(OrderStatus.Completed());
        }

        [Fact]
        public void ReturnErrorIfCompleteWithoutCourierAssignee()
        {
            //Arrange
            var basketId = Guid.NewGuid();
            var location = Location.CreateRandom().Value;
            var order = Order.Create(basketId, location);

            //Act
            var result = order.Value.Complete();

            //Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }
    }
}
