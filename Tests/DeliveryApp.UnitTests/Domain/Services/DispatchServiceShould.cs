using System;
using System.Collections.Generic;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Domain.Services;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Services
{
    public class DispatchServiceShould
    {
        [Fact]
        public void BeCorrectAssignedToCourierWithLessDeliveryTime()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value).Value;
            var courierLocation = Location.Create(3, 3).Value;
            var courierWithFootTransport = Courier.Create("Mark", "Foot", 1, courierLocation).Value;
            var courierWithBikeTransport = Courier.Create("Bob", "Bike", 2, courierLocation).Value;
            var courierWithCarTransport = Courier.Create("Alex", "Car", 3, courierLocation).Value;
            var courierWithSportCarTransport = Courier.Create("Den", "Sport car", 3, courierLocation).Value;
            courierWithSportCarTransport.SetBusy();

            var couriers = new List<Courier>
            {
                courierWithSportCarTransport,
                courierWithBikeTransport,
                courierWithCarTransport,
                courierWithFootTransport
            };
            var dispatchService = new DispatchService();

            // Act
            var result = dispatchService.Dispatch(order, couriers);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.CourierId.Should().NotBeNull();
            result.Value.CourierId.Should().Be(courierWithCarTransport.Id);
            result.Value.Status.Should().Be(OrderStatus.Assign());
            courierWithCarTransport.Status.Should().Be(CourierStatus.Busy());
        }

        [Fact]
        public void BeReturnErrorIfNoFreeCouriers()
        {
            // Arrange
            var order = Order.Create(Guid.NewGuid(), Location.Create(5, 5).Value).Value;
            var courier = Courier.Create("Mark", "Foot", 1, Location.Create(3, 3).Value).Value;
            courier.SetBusy();

            var couriers = new List<Courier> { courier };
            var dispatchService = new DispatchService();

            // Act
            var result = dispatchService.Dispatch(order, couriers);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }
    }
}
