using ApplicationServices.DTO;
using MediatR;

namespace ApplicationServices.Queries
{
    public record GetOrdersByCustomerQuery(string CustomerId) : IRequest<IEnumerable<OrderDto>>;
}