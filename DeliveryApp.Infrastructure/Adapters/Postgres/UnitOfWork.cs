using MediatR;
using Primitives;

namespace DeliveryApp.Infrastructure.Adapters.Postgres
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMediator mediator;

        private bool disposed;

        public UnitOfWork(ApplicationDbContext dbContext, IMediator mediator)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await this.dbContext.SaveChangesAsync(cancellationToken);
            await PublishDomainEventsAsync();
            return true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.dbContext.Dispose();
                }
                this.disposed = true;
            }
        }

        private async Task PublishDomainEventsAsync()
        {
            // Получили агрегаты в которых есть доменные события
            var domainEntities = this.dbContext.ChangeTracker
                .Entries<IAggregateRoot>()
                .Where(x => x.Entity.GetDomainEvents().Any());

            // Переложили в отдельную переменную
            var domainEvents = domainEntities
                .SelectMany(x => x.Entity.GetDomainEvents())
                .ToList();

            // Очистили Domain Event в самих агрегатах (поскольку далее они будут отправлены и больше не нужны)
            domainEntities.ToList()
                .ForEach(entity => entity.Entity.ClearDomainEvents());

            // Отправили в MediatR
            foreach (var domainEvent in domainEvents)
            {
                await this.mediator.Publish(domainEvent);
            }
        }
    }
}
