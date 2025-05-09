using DeliveryApp.Core.Domain.Model.OrderAggregate;

namespace DeliveryApp.Core.Ports
{
    public interface IOrderRepository
    {
        Task AddAsync(Order order);

        void Update(Order order);

        Task<Order> GetAsync(Guid orderId);

        Task<Order> GetFirstInCreatedStatusAsync();

        Task<IReadOnlyCollection<Order>> GetAllInAssignStatus();
    }
}
