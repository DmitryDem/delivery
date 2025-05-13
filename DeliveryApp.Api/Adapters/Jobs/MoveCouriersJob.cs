using DeliveryApp.Core.Application.UseCases.Commands.MoveCouriers;
using MediatR;
using Quartz;

namespace DeliveryApp.Api.Adapters.Jobs
{
    [DisallowConcurrentExecution]
    public class MoveCouriersJob : IJob
    {
        private readonly IMediator mediator;

        public MoveCouriersJob(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var moveCourierToOrderCommand = new MoveCouriersCommand();
            await this.mediator.Send(moveCourierToOrderCommand);
        }
    }
}
