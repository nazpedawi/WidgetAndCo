using ApplicationServices.DTO;
using MediatR;

namespace ApplicationServices.Commands
{
    public record ShipOrderCommand(string OrderId, string CustomerId) : IRequest<OrderDto>;
}