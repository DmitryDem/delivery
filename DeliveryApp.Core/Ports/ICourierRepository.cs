using DeliveryApp.Core.Domain.Model.CourierAggregate;
using Primitives;

namespace DeliveryApp.Core.Ports
{
    public interface ICourierRepository : IRepository<Courier>
    {
        Task AddAsync(Courier courier);

        void Update(Courier courier);

        Task<Courier> GetAsync(Guid courierId);

        IEnumerable<Courier> GetAllInFreeStatus();
    }
}
