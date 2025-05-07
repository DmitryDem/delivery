using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.OrderAggregate;
using Primitives;

namespace DeliveryApp.Core.Domain.Services
{
    public interface IDispatchService
    {
        Result<Order, Error> Dispatch(Order order, List<Courier> couriers);
    }
}
