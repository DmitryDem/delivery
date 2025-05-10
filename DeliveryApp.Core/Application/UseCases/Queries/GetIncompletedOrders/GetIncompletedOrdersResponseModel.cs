namespace DeliveryApp.Core.Application.UseCases.Queries.GetIncompletedOrders
{
    public class GetIncompletedOrdersResponseModel
    {
        public GetIncompletedOrdersResponseModel(List<OrderModel> orders)
        {
            Orders = orders;
        }

        public List<OrderModel> Orders { get; }
    }
}
