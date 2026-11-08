using ApplicationServices.DTO;
using MediatR;

namespace ApplicationServices.Commands
{
    public record PlaceOrderCommand(string CustomerId, List<OrderLineDto> Items, string? OrderId = null) : IRequest<string>;
}