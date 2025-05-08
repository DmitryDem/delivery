namespace DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers
{
    public class GetBusyCouriersResponseModel
    {
        public GetBusyCouriersResponseModel(List<CourierModel> couriers)
        {
            Couriers = couriers;
        }

        public List<CourierModel> Couriers { get; }
    }
}
