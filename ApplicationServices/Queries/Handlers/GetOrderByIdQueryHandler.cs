using ApplicationServices.DTO;
using DataLayer.Repo.Interfaces;
using MediatR;

namespace ApplicationServices.Queries.Handlers
{
    public class GetOrderByIdQueryHandler(IReadOrderRepository orders)
        : IRequestHandler<GetOrderByIdQuery, OrderDto?>
    {
        public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            var order = await orders.GetByIdAsync(request.OrderId, request.CustomerId);
            return order is null ? null : OrderDto.From(order);
        }
    }
}