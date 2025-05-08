namespace DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers
{
    public record CourierModel(Guid Id, string Name, LocationModel Location);
}
