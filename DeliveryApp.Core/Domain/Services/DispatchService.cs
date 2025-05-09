using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services
{
    public class DispatchService : IDispatchService
    {
        public Result<Order, Error> Dispatch(Order order, List<Courier> couriers)
        {
            var courier = couriers
                .Where(c => c.Status == CourierStatus.Free())
                .OrderBy(c => c.CalculateTimeToLocation(order.Location))
                .FirstOrDefault();

            if (courier == null)
            {
                return Errors.NoAvailableCourier();
            }

            var result = courier.SetBusy();
            return !result.IsSuccess ? result.Error : order.Assignee(courier);
        }

        public static class Errors
        {
            public static Error NoAvailableCourier()
            {
                return new Error($"{nameof(Order).ToLowerInvariant()}.no.available.couriers", "No available couriers for order assign");
            }
        }
    }
}
