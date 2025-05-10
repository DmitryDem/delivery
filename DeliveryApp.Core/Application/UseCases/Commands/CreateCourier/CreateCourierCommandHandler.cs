using CSharpFunctionalExtensions;
using DeliveryApp.Core.Domain.Model.CourierAggregate;
using DeliveryApp.Core.Domain.Model.SharedKernel;
using DeliveryApp.Core.Ports;
using MediatR;
using Primitives;

namespace DeliveryApp.Core.Application.UseCases.Commands.CreateCourier
{
    public class CreateCourierCommandHandler : IRequestHandler<CreateCourierCommand, UnitResult<Error>>
    {
        private readonly ICourierRepository courierRepository;
        private readonly IUnitOfWork unitOfWork;

        public CreateCourierCommandHandler(
            ICourierRepository courierRepository,
            IUnitOfWork unitOfWork)
        {
            this.courierRepository = courierRepository ?? throw new ArgumentNullException(nameof(courierRepository));
            this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<UnitResult<Error>> Handle(CreateCourierCommand request, CancellationToken cancellationToken)
        {
            var courier = Courier.Create(request.Name, GetTransportName(request.Speed), (byte)request.Speed, Location.CreateRandom().Value);

            if (courier.IsFailure)
            {
                return UnitResult.Failure(courier.Error);
            }

            await this.courierRepository.AddAsync(courier.Value);
            await this.unitOfWork.SaveChangesAsync(cancellationToken);
            return UnitResult.Success<Error>();
        }

        private static string GetTransportName(int speed)
        {
            switch (speed)
            {
                case 1:
                    return "Foot";
                case 2:
                    return "Bike";
                case 3:
                    return "Car";
            }

            return null;
        }
    }
}
