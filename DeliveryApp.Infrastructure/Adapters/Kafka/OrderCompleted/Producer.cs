using Confluent.Kafka;
using DeliveryApp.Core.Domain.Model.OrderAggregate.DomainEvents;
using DeliveryApp.Core.Ports;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OrderStatusChanged;

namespace DeliveryApp.Infrastructure.Adapters.Kafka.OrderCompleted
{
    public class Producer : IMessageBusProducer
    {
        private readonly ProducerConfig config;
        private readonly string topicName;

        public Producer(IOptions<Settings> options)
        {
            if (string.IsNullOrWhiteSpace(options.Value.MessageBrokerHost))
            {
                throw new ArgumentException(nameof(options.Value.MessageBrokerHost));
            }

            if (string.IsNullOrWhiteSpace(options.Value.OrderStatusChangedTopic))
            {
                throw new ArgumentException(nameof(options.Value.OrderStatusChangedTopic));
            }

            this.config = new ProducerConfig
            {
                BootstrapServers = options.Value.MessageBrokerHost
            };

            this.topicName = options.Value.OrderStatusChangedTopic;
        }

        public async Task Publish(OrderCompletedDomainEvent notification, CancellationToken cancellationToken)
        {
            // Перекладываем данные из Domain Event в Integration Event

            var orderCompleteIntegrationEvent = new OrderStatusChangedIntegrationEvent
            {
                OrderId = notification.OrderId.ToString(),
                OrderStatus = OrderStatus.Completed
            };

            // Создаем сообщение для Kafka
            var message = new Message<string, string>
            {
                Key = notification.EventId.ToString(),
                Value = JsonConvert.SerializeObject(orderCompleteIntegrationEvent)
            };

            try
            {
                // Отправляем сообщение в Kafka
                using var producer = new ProducerBuilder<string, string>(this.config).Build();
                var dr = await producer.ProduceAsync(this.topicName, message, cancellationToken);
                Console.WriteLine($"Delivered '{dr.Value}' to '{dr.TopicPartitionOffset}'");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Delivery failed: {e.Error.Reason}");
            }
        }
    }
}
