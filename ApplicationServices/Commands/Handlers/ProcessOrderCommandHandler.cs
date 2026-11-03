using DataLayer.Repo.Interfaces;
using MediatR;

namespace ApplicationServices.Commands.Handlers
{
    public class ProcessOrderCommandHandler(IWriteOrderRepository orders)
        : IRequestHandler<ProcessOrderCommand>
    {
        public async Task Handle(ProcessOrderCommand request, CancellationToken cancellationToken)
        {
            var order = request.Order;

            // At-least-once delivery: skip if this order was already stored
            if (await orders.GetByIdAsync(order.Id, order.CustomerId) is not null)
                return;

            order.MarkProcessed();
            await orders.SaveAsync(order);
        }
    }
}