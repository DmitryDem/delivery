using CSharpFunctionalExtensions;
using DeliveryApp.Core.Ports;
using GeoApp.Api;
using Grpc.Core;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Primitives;
using Location = DeliveryApp.Core.Domain.Model.SharedKernel.Location;

namespace DeliveryApp.Infrastructure.Adapters.Grpc.GeoService
{
    public class GeoClient : IGeoClient
    {
        private readonly MethodConfig methodConfig;
        private readonly SocketsHttpHandler socketsHttpHandler;
        private readonly string url;

        public GeoClient(string geoServiceGrpcHost)
        {
            if (string.IsNullOrWhiteSpace(geoServiceGrpcHost))
            {
                throw new ArgumentException(nameof(geoServiceGrpcHost));
            }

            this.url = geoServiceGrpcHost;
            this.socketsHttpHandler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                EnableMultipleHttp2Connections = true
            };

            this.methodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { StatusCode.Unavailable }
                }
            };
        }

        public async Task<Result<Location, Error>> GetLocation(string street, CancellationToken cancellationToken)
        {
            using var channel = GrpcChannel.ForAddress(this.url, new GrpcChannelOptions
            {
                HttpHandler = this.socketsHttpHandler,
                ServiceConfig = new ServiceConfig
                {
                    MethodConfigs = { this.methodConfig }
                }
            });

            var client = new Geo.GeoClient(channel);
            var reply = await client.GetGeolocationAsync(
                            new GetGeolocationRequest { Street = street },
                            null,
                            DateTime.UtcNow.AddSeconds(5),
                            cancellationToken);

            return Location.Create(reply.Location.X, reply.Location.Y);
        }
    }
}
