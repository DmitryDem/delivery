using CSharpFunctionalExtensions;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateCourier
{
    public class CreateCourierCommand : IRequest<UnitResult<Error>>
    {
        public CreateCourierCommand(string name, int speed)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if(speed <= 0) throw new ArgumentOutOfRangeException(nameof(speed));

            Name = name;
            Speed = speed;
        }

        public string Name { get; }

        public int Speed { get; }
    }
}
