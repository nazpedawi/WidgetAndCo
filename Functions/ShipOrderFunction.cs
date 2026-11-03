using ApplicationServices.Commands;
using ApplicationServices.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Functions
{
    public class ShipOrderFunction(IMediator mediator)
    {
        [Function(nameof(ShipOrder))]
        public async Task<IActionResult> ShipOrder(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders/{customerId}/{orderId}/ship")] HttpRequest req,
            string customerId,
            string orderId)
        {
            try
            {
                var order = await mediator.Send(new ShipOrderCommand(orderId, customerId));
                return new OkObjectResult(order);
            }
            catch (NotFoundException ex)
            {
                return new NotFoundObjectResult(new { error = ex.Message });
            }
            catch (ValidationException ex)
            {
                return new BadRequestObjectResult(new { error = ex.Message });
            }
        }
    }
}