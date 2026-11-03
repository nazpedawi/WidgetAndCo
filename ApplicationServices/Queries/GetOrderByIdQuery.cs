using ApplicationServices.DTO;
using MediatR;

namespace ApplicationServices.Queries
{
    public record GetOrderByIdQuery(string OrderId, string CustomerId) : IRequest<OrderDto?>;
}