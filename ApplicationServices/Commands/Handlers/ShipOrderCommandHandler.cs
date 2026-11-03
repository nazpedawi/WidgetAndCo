using ApplicationServices.DTO;
using ApplicationServices.Exceptions;
using DataLayer.Repo.Interfaces;
using MediatR;

namespace ApplicationServices.Commands.Handlers
{
    public class ShipOrderCommandHandler(IWriteOrderRepository orders, IPublisher publisher)
        : IRequestHandler<ShipOrderCommand, OrderDto>
    {
        public async Task<OrderDto> Handle(ShipOrderCommand request, CancellationToken cancellationToken)
        {
            var order = await orders.GetByIdAsync(request.OrderId, request.CustomerId)
                ?? throw new NotFoundException($"Order '{request.OrderId}' not found.");

            try
            {
                order.Ship(DateTime.UtcNow);   // domain rules live in the entity
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                throw new ValidationException(ex.Message);
            }

            await orders.SaveAsync(order);

            foreach (var domainEvent in order.GetDomainEvents())
                await publisher.Publish(domainEvent, cancellationToken);
            order.ClearDomainEvents();

            return OrderDto.From(order);
        }
    }
}