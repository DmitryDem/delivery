using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.Model.CourierAggregate
{
    public class CourierShould
    {
        public static IEnumerable<object[]> GetInvalidCourierData()
        {
            yield return [null, "feet", 1, Location.Create(1, 1).Value];
            yield return [string.Empty, "feet", 1, Location.Create(1, 1).Value];
            yield return ["Alex", null, 1, Location.Create(1, 1).Value];
            yield return ["Alex", string.Empty, 1, Location.Create(1, 1).Value] ;
            yield return ["Alex", "feet", Speed.Min - 1, Location.Create(1, 1).Value];
            yield return ["Alex", "feet", Speed.Max + 1, Location.Create(1, 1).Value];
        }

        public static IEnumerable<object[]> GetCourierTimeToLocationData()
        {
            yield return [Location.Create(5, 6).Value, 1, Location.Create(2, 9).Value, 6, 0.01];
            yield return [Location.Create(5, 6).Value, 2, Location.Create(2, 9).Value, 3, 0.01];
            yield return [Location.Create(8, 9).Value, 3, Location.Create(9, 8).Value, 0.66, 0.01];
            yield return [Location.Create(4, 3).Value, 3, Location.Create(3, 3).Value, 0.33, 0.01];
        }

        public static IEnumerable<object[]> GetCourierMoveToLocationData()
        {
            yield return [Location.Create(2, 9).Value, 1, Location.Create(10, 1).Value, Location.Create(3, 9).Value];
            yield return [Location.Create(2, 9).Value, 3, Location.Create(10, 1).Value, Location.Create(5, 9).Value];
            yield return [Location.Create(6, 8).Value, 3, Location.Create(6, 9).Value, Location.Create(6, 9).Value];
            yield return [Location.Create(9, 5).Value, 2, Location.Create(9, 1).Value, Location.Create(9, 3).Value];
        }

        [Fact]
        public void BeCorrectWhenParamsIsCorrectOnCreated()
        {
            //Arrange

            //Act
            var courier = Courier.Create("Alex", "feet", 1, Location.Create(1, 1).Value);

            //Assert
            courier.IsSuccess.Should().BeTrue();
            courier.Value.Name.Should().Be("Alex");
            courier.Value.Transport.Should().NotBeNull();
            courier.Value.Location.Should().NotBeNull();
            courier.Value.Status.Should().Be(CourierStatus.Free());
        }

        [Theory]
        [MemberData(nameof(GetInvalidCourierData))]
        public void ReturnErrorWhenParamsIsIncorrectOnCreated(string name, string transportName, byte transportSpeed, Location location)
        {
            //Arrange

            //Act
            var result = Courier.Create(name, transportName, transportSpeed, location);

            //Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeSetInBusyStatusIfCurrentStatusIsFree()
        {
            //Arrange
            var courier = Courier.Create("Alex", "feet", 1, Location.Create(1, 1).Value).Value;

            //Act
            var result = courier.SetBusy();

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Status.Should().Be(CourierStatus.Busy());
        }

        [Fact]
        public void ReturnErrorIfSetBusyStatusIfCurrentStatusIsNotFree()
        {
            //Arrange
            var courier = Courier.Create("Alex", "feet", 1, Location.Create(1, 1).Value).Value;
            courier.SetBusy();

            //Act
            var result = courier.SetBusy();

            //Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
        }

        [Fact]
        public void BeSetFreeStatus()
        {
            //Arrange
            var courier = Courier.Create("Alex", "feet", 1, Location.Create(1, 1).Value).Value;
            courier.SetBusy();

            //Act
            var result = courier.SetFree();

            //Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Status.Should().Be(CourierStatus.Free());
        }

        [Theory]
        [MemberData(nameof(GetCourierTimeToLocationData))]
        public void ReturnCorrectTimeToLocation(Location courierLocation, byte transportSpeed, Location orderLocation, float expected, float epsilon)
        {
            //Arrange
            var courier = Courier.Create("Alex", "some", transportSpeed, courierLocation).Value;

            //Act
            var time = courier.CalculateTimeToLocation(orderLocation);

            //Assert
            var result = time - expected;
            result.Should().BeGreaterThanOrEqualTo(0);
            result.Should().BeLessThan(epsilon);
        }

        [Theory]
        [MemberData(nameof(GetCourierMoveToLocationData))]
        public void MoveCorrectToLocationTarget(Location currentCourierLocation, byte transportSpeed, Location targetOrderLocation, Location expected)
        {
            //Arrange
            var courier = Courier.Create("Alex", "some", transportSpeed, currentCourierLocation).Value;

            //Act
            courier.Move(targetOrderLocation);

            //Assert
            courier.Location.Should().Be(expected);
        }
    }
}
