using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services
{
    public class DispatchService : IDispatchService
    {
        public Result<Courier, Error> Dispatch(Order order, List<Courier> couriers)
        {
            var courier = couriers
                .Where(c => c.Status == CourierStatus.Free())
                .OrderBy(c => c.CalculateTimeToLocation(order.Location))
                .FirstOrDefault();

            if (courier == null)
            {
                return Errors.NoAvailableCourier();
            }

            var assigneeResult = order.Assignee(courier);
            if (!assigneeResult.IsSuccess)
            {
                return assigneeResult.Error;
            }

            return courier.SetBusy();
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
