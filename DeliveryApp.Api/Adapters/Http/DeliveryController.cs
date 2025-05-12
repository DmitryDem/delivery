using DeliveryApp.Core.Application.UseCases.Commands.CreateCourier;
using DeliveryApp.Core.Application.UseCases.Commands.CreateOrder;
using DeliveryApp.Core.Application.UseCases.Queries.GetAllCouriers;
using DeliveryApp.Core.Application.UseCases.Queries.GetIncompletedOrders;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using OpenApi.Controllers;
using OpenApi.Models;

namespace DeliveryApp.Api.Adapters.Http
{
    public class DeliveryController : DefaultApiController
    {
        private readonly IMediator mediator;

        public DeliveryController(IMediator mediator)
        {
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public override async Task<IActionResult> CreateCourier(NewCourier newCourier)
        {
            var command = new CreateCourierCommand(newCourier.Name, newCourier.Speed);
            var response = await this.mediator.Send(command);
            if (response.IsFailure)
            {
                return this.BadRequest(response.Error);
            }

            return this.Created();
        }

        public override async Task<IActionResult> CreateOrder()
        {
            //var command = new CreateOrderCommand(Guid.NewGuid(), "Айтишная");
            var command = new CreateOrderCommand(Guid.NewGuid(), "Random");
            var response = await this.mediator.Send(command);
            if (response.IsFailure)
            {
                return this.BadRequest(response.Error);
            }

            return this.Created();
        }

        public override async Task<IActionResult> GetCouriers()
        {
            var query = new GetAllCouriersQuery();
            var response = await this.mediator.Send(query);

            if (response.Couriers.Count == 0)
            {
                return this.NotFound();
            }

            return this.Ok(response.Couriers);
        }
        
        public override async Task<IActionResult> GetOrders()
        {
            var orders = new GetIncompletedOrdersQuery();
            var response = await this.mediator.Send(orders);

            if (response.Orders.Count == 0)
            {
                return this.NotFound();
            }

            return this.Ok(response.Orders);
        }
    }
}
