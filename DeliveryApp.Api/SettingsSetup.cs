using DeliveryApp.Infrastructure;
using Microsoft.Extensions.Options;

namespace DeliveryApp.Api;

public class SettingsSetup : IConfigureOptions<Settings>
{
    private readonly IConfiguration configuration;

    public SettingsSetup(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public void Configure(Settings options)
    {
        options.ConnectionString = this.configuration["CONNECTION_STRING"];
        options.GeoServiceGrpcHost = this.configuration["GEO_SERVICE_GRPC_HOST"];
        options.MessageBrokerHost = this.configuration["MESSAGE_BROKER_HOST"];
        options.OrderStatusChangedTopic = this.configuration["ORDER_STATUS_CHANGED_TOPIC"];
        options.BasketConfirmedTopic = this.configuration["BASKET_CONFIRMED_TOPIC"];
    }
}