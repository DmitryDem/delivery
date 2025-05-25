using BasketConfirmed;
using Confluent.Kafka;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Infrastructure;
using MediatR;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace DeliveryApp.Api.Adapters.Kafka.BasketConfirmed
{
    public class ConsumerService : BackgroundService
    {
        private readonly IMediator mediator;
        private readonly IConsumer<Ignore, string> consumer;
        private readonly string topic;

        public ConsumerService(IMediator mediator, IOptions<Settings> settings)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            if (string.IsNullOrWhiteSpace(settings.Value.MessageBrokerHost)) throw new ArgumentException(nameof(settings.Value.MessageBrokerHost));
            if (string.IsNullOrWhiteSpace(settings.Value.OrderStatusChangedTopic)) throw new ArgumentException(nameof(settings.Value.OrderStatusChangedTopic));

            var consumerConfig = new ConsumerConfig
                                     {
                                         BootstrapServers = settings.Value.MessageBrokerHost,
                                         GroupId = "DeliveryConsumerGroup",
                                         EnableAutoOffsetStore = false,
                                         EnableAutoCommit = true,
                                         AutoOffsetReset = AutoOffsetReset.Earliest,
                                         EnablePartitionEof = true
                                     };
            this.consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
            this.topic = settings.Value.BasketConfirmedTopic;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.consumer.Subscribe(this.topic);
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                    var consumeResult = this.consumer.Consume(cancellationToken);

                    if (consumeResult.IsPartitionEOF) continue;

                    var basketConfirmedIntegrationEvent =
                        JsonConvert.DeserializeObject<BasketConfirmedIntegrationEvent>(consumeResult.Message.Value);

                    var createOrderCommand = new CreateOrderCommand(Guid.Parse(basketConfirmedIntegrationEvent.BasketId), basketConfirmedIntegrationEvent.Address.Street);
                    var response = await this.mediator.Send(createOrderCommand, cancellationToken);
                    if (!response.IsSuccess) Console.WriteLine(response.Error.Message);

                    try
                    {
                        this.consumer.StoreOffset(consumeResult);
                    }
                    catch (KafkaException e)
                    {
                        Console.WriteLine($"Store Offset error: {e.Error.Reason}");
                    }
                }
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
