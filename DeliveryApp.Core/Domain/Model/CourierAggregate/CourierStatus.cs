using CSharpFunctionalExtensions;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate
{
    public class CourierStatus : ValueObject
    {
        /// <summary>
        /// Дефолтный конструктов для ORM
        /// </summary>
        [ExcludeFromCodeCoverage]
        private CourierStatus()
        {
        }

        private CourierStatus(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return Name;
        }

        public static CourierStatus Free()
        {
            return new CourierStatus(nameof(Free).ToLowerInvariant());
        }

        public static CourierStatus Busy()
        {
            return new CourierStatus(nameof(Busy).ToLowerInvariant());
        }
    }
}
