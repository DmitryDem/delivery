using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetIncompletedOrders
{
    public class GetIncompletedOrdersQueryHandler : IRequestHandler<GetIncompletedOrdersQuery, GetIncompletedOrdersResponseModel>
    {
        private readonly string connectionString;

        public GetIncompletedOrdersQueryHandler(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<GetIncompletedOrdersResponseModel> Handle(GetIncompletedOrdersQuery request, CancellationToken cancellationToken)
        {
            await using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            var command = new Npgsql.NpgsqlCommand("SELECT id, location_x, location_y FROM public.orders WHERE status IN ('created', 'assigned')", connection);
            var reader = await command.ExecuteReaderAsync(cancellationToken);

            var couriers = new List<OrderModel>();
            while (await reader.ReadAsync(cancellationToken))
            {
                var id = reader.GetGuid(0);
                var locationX = reader.GetInt32(1);
                var locationY = reader.GetInt32(2);
                couriers.Add(new OrderModel(id, new LocationModel(locationX, locationY)));
            }

            return new GetIncompletedOrdersResponseModel(couriers);
        }
    }
}
