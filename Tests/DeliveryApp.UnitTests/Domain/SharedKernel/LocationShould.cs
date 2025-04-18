using System.Collections.Generic;
using System.Linq;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using FluentAssertions;
using Xunit;

namespace DeliveryApp.UnitTests.Domain.SharedKernel
{
    public class LocationShould
    {
        public static IEnumerable<object[]> LocationMatrix =>
            from x in Enumerable.Range(Location.Min, Location.Max)
            from y in Enumerable.Range(Location.Min, Location.Max)
            select new object[] { (byte)x, (byte)y };

        public static IEnumerable<object[]> DistanceToTestData()
        {
            yield return [Location.Create(1, 1).Value, Location.Create(10, 1).Value, 9];
            yield return [Location.Create(10, 1).Value, Location.Create(1, 1).Value, 9];
            yield return [Location.Create(1, 1).Value, Location.Create(1, 10).Value, 9];
            yield return [Location.Create(1, 1).Value, Location.Create(10, 10).Value, 18];
            yield return [Location.Create(2, 4).Value, Location.Create(9, 8).Value, 11];
            yield return [Location.Create(8, 10).Value, Location.Create(9, 2).Value, 9];
        }

        [Theory]
        [MemberData(nameof(LocationMatrix))]
        public void BeCorrectWhenParamsIsCorrectOnCreated(byte x, byte y)
        {
            //Arrange

            //Act
            var location = Location.Create(x, y);

            //Assert
            location.IsSuccess.Should().BeTrue();
            location.Value.X.Should().Be(x);
            location.Value.Y.Should().Be(y);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(0, 0)]
        [InlineData(1, 11)]
        [InlineData(11, 1)]
        [InlineData(11, 11)]
        public void ReturnErrorWhenParamsIsIncorrectOnCreated(byte x, byte y)
        {
            //Arrange

            //Act
            var location = Location.Create(x, y);

            //Assert
            location.IsSuccess.Should().BeFalse();
            location.Error.Should().NotBeNull();
        }

        [Theory]
        [MemberData(nameof(DistanceToTestData))]
        public void BeReturnDistance(Location location1, Location location2, byte expected)
        {
            //Arrange

            //Act
            var distance = location1.DistanceTo(location2);

            //Assert
            distance.Should().Be(expected);
        }

        [Fact]

        public void BeEqualsWhenAllPropertiesAreEquals()
        {
            //Arrange
            var location1 = Location.Create(1, 1).Value;
            var location2 = Location.Create(1, 1).Value;

            //Act
            var result = location1.Equals(location2);

            //Assert
            result.Should().BeTrue();
        }


        [Fact]
        public void NotBeEqualsWhenAllPropertiesAreNotEquals()
        {
            //Arrange
            var location1 = Location.Create(1, 2).Value;
            var location2 = Location.Create(2, 1).Value;

            //Act
            var result = location1.Equals(location2);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void BeRandomCreated()
        {
            //Arrange
            var location = Location.CreateRandom();

            //Act

            //Assert
            location.Should().NotBeNull();
            location.IsSuccess.Should().BeTrue();
            location.Value.X.Should().BeInRange(Location.Min, Location.Max);
            location.Value.Y.Should().BeInRange(Location.Min, Location.Max);
        }
    }
}
