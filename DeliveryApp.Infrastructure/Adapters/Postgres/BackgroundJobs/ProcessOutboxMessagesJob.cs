using MediatR;
using Quartz;
using DeliveryApp.Infrastructure.Adapters.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using Primitives;

namespace DeliveryApp.Infrastructure.Adapters.Postgres.BackgroundJobs
{
    [DisallowConcurrentExecution]
    public class ProcessOutboxMessagesJob : IJob
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IMediator mediator;

        public ProcessOutboxMessagesJob(ApplicationDbContext dbContext, IMediator mediator)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // Получаем все DomainEvents, которые еще не были отправлены (где ProcessedOnUtc == null)
            var outboxMessages = await this.dbContext
                                     .Set<OutboxMessage>()
                                     .Where(m => m.ProcessedOnUtc == null)
                                     .OrderBy(o => o.OccurredOnUtc)
                                     .Take(20)
                                     .ToListAsync(context.CancellationToken);

            // Если такие есть, то перебираем их в цикле
            if (outboxMessages.Any())
            {
                foreach (var outboxMessage in outboxMessages)
                {
                    // Настройки сериализатора
                    var settings = new JsonSerializerSettings
                                       {
                                           ContractResolver = new DefaultContractResolver(),
                                           TypeNameHandling = TypeNameHandling.All
                                       };

                    // Десериализуем запись из OutboxMessages в DomainEvent
                    var domainEvent = JsonConvert.DeserializeObject<DomainEvent>(outboxMessage.Content, settings);

                    // Отправляем
                    await this.mediator.Publish(domainEvent, context.CancellationToken);

                    // Если предыдущий метод не вернул ошибку, значит отправка была успешной
                    // Ставим дату отправки, это будет признаком, что сообщение отправлять больше не нужно 
                    outboxMessage.ProcessedOnUtc = DateTime.UtcNow;
                }

                // Сохраняем изменения
                await this.dbContext.SaveChangesAsync();
            }
        }

    }
}
