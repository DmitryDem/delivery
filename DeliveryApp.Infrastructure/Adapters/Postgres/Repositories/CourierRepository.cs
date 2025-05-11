using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Ports;
using Microsoft.EntityFrameworkCore;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.Repositories
{
    public class CourierRepository : ICourierRepository
    {
        private readonly ApplicationDbContext dbContext;

        public CourierRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task AddAsync(Courier courier)
        {
            await this.dbContext.Couriers.AddAsync(courier);
        }

        public void Update(Courier courier)
        {
            this.dbContext.Couriers.Update(courier);
        }

        public Task<Courier> GetAsync(Guid courierId)
        {
            return this.dbContext.Couriers
                .Include(c => c.Transport)
                .SingleOrDefaultAsync(c => c.Id == courierId);
        }

        public async Task<IReadOnlyCollection<Courier>> GetAllInFreeStatus()
        {
            return await this.dbContext.Couriers
                .Where(c => c.Status.Name == CourierStatus.Free().Name)
                .Include(c => c.Transport)
                .ToListAsync();
        }
    }
}
