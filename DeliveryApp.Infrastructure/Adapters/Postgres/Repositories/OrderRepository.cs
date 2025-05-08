using DeliveryApp.Core.Domain.Model.OrderAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext dbContext;

        public OrderRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAsync(Order order)
        {
            await this.dbContext.Orders.AddAsync(order);
        }

        public void Update(Order order)
        {
            this.dbContext.Orders.Update(order);
        }

        public async Task<Order> GetAsync(Guid orderId)
        {
            return await this.dbContext
                .Orders
                .SingleOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<Order> GetFirstInCreatedStatusAsync()
        {
            return await this.dbContext
                       .Orders
                       .FirstOrDefaultAsync(o => o.Status.Name == OrderStatus.Created().Name);
        }

        public IEnumerable<Order> GetAllInAssignStatus()
        {
            return this.dbContext
                       .Orders
                       .Where(o => o.Status.Name == OrderStatus.Assigned().Name);
        }
    }
}
