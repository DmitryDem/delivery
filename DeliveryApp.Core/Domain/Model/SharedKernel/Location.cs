using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using Primitives;

namespace DeliveryApp.Core.Domain.Model.SharedKernel
{
    public class Location : ValueObject
    {
        public const byte Min = 1;
        public const byte Max = 10;

        [ExcludeFromCodeCoverage]
        private Location()
        {
        }

        private Location(byte x, byte y)
            : this()
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Координата X
        /// </summary>
        public byte X { get; }

        /// <summary>
        /// Координата Y
        /// </summary>
        public byte Y { get; }

        /// <summary>
        /// Создание объекта Location
        /// </summary>
        /// <param name="x">Координата X</param>
        /// <param name="y">Координата Y</param>
        /// <returns>
        /// Объект Location
        /// </returns>
        public static Result<Location, Error> Create(byte x, byte y)
        {
            if (x is < Min or > Max)
            {
                return GeneralErrors.ValueIsInvalid(nameof(x));
            }

            if (y is < Min or > Max)
            {
                return GeneralErrors.ValueIsInvalid(nameof(y));
            }

            return new Location(x, y);
        }

        /// <summary>
        /// Создание объекта Location
        /// </summary>
        /// <returns>
        /// Объект Location
        /// </returns>
        public static Result<Location, Error> CreateRandom()
        {
            var random = new Random();
            var x = (byte)random.Next(Min, Max + 1);
            var y = (byte)random.Next(Min, Max + 1);
            return new Location(x, y);
        }

        public byte DistanceTo(Location location)
        {
            var xDistance = (byte)Math.Abs(location.X - X);
            var yDistance = (byte)Math.Abs(location.Y - Y);
            return (byte)(xDistance + yDistance);
        }

        /// <summary>
        /// Перегрузка идентичности
        /// </summary>
        /// <returns>
        /// Список свойств для сравнения с идентичности
        /// </returns>
        [ExcludeFromCodeCoverage]
        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return X;
            yield return Y;
        }
    }
}
