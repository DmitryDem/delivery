using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetIncompletedOrders
{
    public class GetIncompletedOrdersQuery : IRequest<GetIncompletedOrdersResponseModel>
    {
    }
}
