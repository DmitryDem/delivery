using CSharpFunctionalExtensions;
using System.Diagnostics.CodeAnalysis;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate
{
    public class OrderStatus : ValueObject
    {
        /// <summary>
        /// Дефолтный конструктов для ORM
        /// </summary>
        [ExcludeFromCodeCoverage]
        private OrderStatus()
        {
        }

        private OrderStatus(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }

        protected override IEnumerable<IComparable> GetEqualityComponents()
        {
            yield return Name;
        }

        public static OrderStatus Created()
        {
            return new OrderStatus(nameof(Created).ToLowerInvariant());
        }

        public static OrderStatus Assigned()
        {
            return new OrderStatus(nameof(Assigned).ToLowerInvariant());
        }

        public static OrderStatus Completed()
        {
            return new OrderStatus(nameof(Completed).ToLowerInvariant());
        }
    }
}
