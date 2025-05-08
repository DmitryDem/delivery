using MediatR;

namespace DeliveryApp.Core.Application.UseCases.Queries.GetBusyCouriers
{
    public class GetBusyCouriersQueryHandler : IRequestHandler<GetBusyCouriersQuery, GetBusyCouriersResponseModel>
    {
        private readonly string connectionString;

        public GetBusyCouriersQueryHandler(string connectionString)
        {
            this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<GetBusyCouriersResponseModel> Handle(GetBusyCouriersQuery request, CancellationToken cancellationToken)
        {
            await using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            var command = new Npgsql.NpgsqlCommand("SELECT id, name, location_x, location_y FROM public.couriers WHERE status = 'busy'", connection);
            var reader = await command.ExecuteReaderAsync(cancellationToken);

            var couriers = new List<CourierModel>();
            while (await reader.ReadAsync(cancellationToken))
            {
                var id = reader.GetGuid(0);
                var name = reader.GetString(1);
                var locationX = reader.GetInt32(2);
                var locationY = reader.GetInt32(3);
                couriers.Add(new CourierModel(id, name, new LocationModel(locationX, locationY)));
            }

            return new GetBusyCouriersResponseModel(couriers);
        }
    }
}
