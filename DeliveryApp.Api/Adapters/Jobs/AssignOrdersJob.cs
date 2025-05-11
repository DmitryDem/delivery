using DeliveryApp.Core.Application.UseCases.Commands.AssignOrderToCourier;
using MediatR;
using Quartz;

namespace DeliveryApp.Api.Adapters.Jobs
{
    [DisallowConcurrentExecution]
    public class AssignOrdersJob : IJob
    {
        private readonly IMediator mediator;

        public AssignOrdersJob(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var assignOrdersCommand = new AssignOrderToCourierCommand();
            await this.mediator.Send(assignOrdersCommand);
        }
    }
}
