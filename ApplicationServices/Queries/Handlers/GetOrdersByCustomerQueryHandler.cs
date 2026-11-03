using ApplicationServices.DTO;
using DataLayer.Repo.Interfaces;
using MediatR;

namespace ApplicationServices.Queries.Handlers
{
    public class GetOrdersByCustomerQueryHandler(IReadOrderRepository orders)
        : IRequestHandler<GetOrdersByCustomerQuery, IEnumerable<OrderDto>>
    {
        public async Task<IEnumerable<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
        {
            var result = await orders.GetByCustomerAsync(request.CustomerId);
            return result.Select(OrderDto.From);
        }
    }
}