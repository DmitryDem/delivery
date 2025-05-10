namespace DeliveryApp.Core.Application.UseCases.Queries
{
    public class GetCouriersResponseModel
    {
        public GetCouriersResponseModel(List<CourierModel> couriers)
        {
            Couriers = couriers;
        }

        public List<CourierModel> Couriers { get; }
    }
}
