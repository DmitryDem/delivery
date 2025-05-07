using Primitives;
using System.Diagnostics.CodeAnalysis;
using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.SharedKernel;

namespace DeliveryApp.Core.Domain.Model.CourierAggregate
{
    public class Courier : Aggregate<Guid>
    {
        /// <summary>
        /// Дефолтный конструктов для ORM
        /// </summary>
        [ExcludeFromCodeCoverage]
        private Courier()
        {
        }

        /// <summary>
        /// Конструктор создания курьера
        /// </summary>
        /// <param name="name">Имя курьера</param>
        /// <param name="transport">Транспорт курьера</param>
        /// <param name="location">Гео позиция курьера</param>
        private Courier(string name, Transport transport, Location location)
        {
            Id = Guid.NewGuid();
            Name = name;
            Transport = transport;
            Location = location;
            Status = CourierStatus.Free();
        }

        public string Name { get; private set; }

        public Transport Transport { get; }

        public Location Location { get; private set; }

        public CourierStatus Status { get; private set; }

        public static Result<Courier, Error> Create(string name, string transportName, byte transportSpeed, Location location)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Errors.CourierNameIsRequired();
            }

            var speed = Speed.Create(transportSpeed);
            if (!speed.IsSuccess)
            {
                return speed.Error;
            }

            var transport = Transport.Create(transportName, speed.Value);

            if (!transport.IsSuccess)
            {
                return transport.Error;
            }

            if (location == null)
            {
                return Errors.CourierLocationIsRequired();
            }

            return new Courier(name, transport.Value, location);
        }

        public Result<Courier, Error> SetBusy()
        {
            if (Status != CourierStatus.Free())
            {
                return Errors.CourierIsNotFree(this);
            }

            Status = CourierStatus.Busy();
            return this;
        }

        public Result<Courier, Error> SetFree()
        {
            Status = CourierStatus.Free();
            return this;
        }

        public float CalculateTimeToLocation(Location location)
        {
            var distance = Location.DistanceTo(location);
            var speed = Transport.Speed.Value;
            return (float)distance / speed;
        }

        public void Move(Location target)
        {
            Location = Transport.Move(Location, target);
        }

        public static class Errors
        {
            public static Error CourierNameIsRequired()
            {
                return new Error($"{nameof(Courier).ToLowerInvariant()}.name.is.required", "Courier name is required");
            }

            public static Error CourierLocationIsRequired()
            {
                return new Error($"{nameof(Courier).ToLowerInvariant()}.location.is.required", "Courier location is required");
            }

            public static Error CourierIsNotFree(Courier courier)
            {
                return new Error($"{nameof(Courier).ToLowerInvariant()}.status.is.not.free", $"Courier is not free. Current courier status {courier.Status}");
            }
        }
    }
}
