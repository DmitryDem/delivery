using Primitives;
using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;

namespace DeliveryApp.Core.Domain.Model.OrderAggregate
{
    public class Order : Aggregate<Guid>
    {
        private static readonly HashSet<OrderStatus> AssignStatusTransitions =
            [
                OrderStatus.Created(),
                OrderStatus.Assign()
            ];

        private static readonly HashSet<OrderStatus> CompleteStatusTransitions =
            [
                OrderStatus.Assign()
            ];

        /// <summary>
        /// Дефолтный конструктов для ORM
        /// </summary>
        [ExcludeFromCodeCoverage]
        private Order()
        {
        }

        /// <summary>
        /// Конструктор создания заказа
        /// </summary>
        /// <param name="basketId">В качестве идентификатора заказа будет выступать идентификатор корзины</param>
        /// <param name="location">Гео координаты заказа</param>
        private Order(Guid basketId, Location location)
        {
            Id = basketId;
            Location = location;
            Status = OrderStatus.Created();
        }

        public Location Location { get; private set; }

        public OrderStatus Status { get; private set; }

        public Guid? CourierId { get; set; }

        public static Result<Order, Error> Create(Guid basketId, Location location)
        {
            if (basketId == Guid.Empty)
            {
                return Errors.OrderIdIsRequired();
            }

            if (location == null)
            {
                return Errors.OrderLocationIsRequired();
            }

            return new Order(basketId, location);
        }

        public Result<Order, Error> Assignee(Courier courier)
        {
            if (!AssignStatusTransitions.Contains(Status))
            {
                return Errors.InvalidAssigneeTransitionOrderStatus();
            }

            if (courier == null)
            {
                return Errors.OrderCourierIsRequired();
            }

            CourierId = courier.Id;
            Status = OrderStatus.Assign();
            return this;
        }

        public Result<Order, Error> Complete()
        {
            if (!CompleteStatusTransitions.Contains(Status))
            {
                return Errors.InvalidCompleteTransitionOrderStatus();
            }

            Status = OrderStatus.Completed();
            return this;
        }

        public static class Errors
        {
            public static Error OrderIdIsRequired()
            {
                return new Error($"{nameof(Order).ToLowerInvariant()}.id.is.required", "Order id is required");
            }

            public static Error OrderLocationIsRequired()
            {
                return new Error($"{nameof(Order).ToLowerInvariant()}.location.is.required", "Order location is required");
            }

            public static Error OrderCourierIsRequired()
            {
                return new Error($"{nameof(Order).ToLowerInvariant()}.courier.is.required", "Order courier is required");
            }

            public static Error InvalidAssigneeTransitionOrderStatus()
            {
                return new Error($"{nameof(Order).ToLowerInvariant()}.status.invalid", $"Order status is invalid. For transition to assignee status the order should be in the following statuses: {string.Join(",", AssignStatusTransitions.Select(status => status.Name))}");
            }

            public static Error InvalidCompleteTransitionOrderStatus()
            {
                return new Error($"{nameof(Order).ToLowerInvariant()}.status.invalid", $"Order status is invalid. For transition to complete status the order should be in the following statuses: {string.Join(",", CompleteStatusTransitions.Select(status => status.Name))}");
            }
        }
    }
}
