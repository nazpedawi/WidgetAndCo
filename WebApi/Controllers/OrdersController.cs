using ApplicationServices.Commands;
using ApplicationServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrdersController(IMediator mediator) : ControllerBase
    {
        // Write side: validate + queue, return immediately
        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderCommand command)
        {
            string orderId = await mediator.Send(command);
            return AcceptedAtAction(nameof(GetById),
                new { orderId, customerId = command.CustomerId },
                new { orderId, status = "Queued" });
        }

        // Read side
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetById(string orderId, [FromQuery] string customerId)
        {
            var order = await mediator.Send(new GetOrderByIdQuery(orderId, customerId));
            return order is null ? NotFound() : Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetByCustomer([FromQuery] string customerId)
            => Ok(await mediator.Send(new GetOrdersByCustomerQuery(customerId)));

        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics([FromQuery] DateTime from, [FromQuery] DateTime to)
            => Ok(await mediator.Send(new GetProcessingMetricsQuery(from, to)));
    }
}