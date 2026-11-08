using DataLayer.Repo.Interfaces;
using MediatR;
using ApplicationServices.Exceptions;
using Microsoft.Extensions.Logging;

namespace ApplicationServices.Commands.Handlers
{
     public class ProcessOrderCommandHandler(
        IWriteOrderRepository orders,
        IWriteProductRepository products,
        ILogger<ProcessOrderCommandHandler> logger)
        : IRequestHandler<ProcessOrderCommand>
    {
        public async Task Handle(ProcessOrderCommand request, CancellationToken cancellationToken)
        {
            var order = request.Order;

            // At-least-once delivery
            if (await orders.GetByIdAsync(order.Id, order.CustomerId) is not null)
            {
                logger.LogInformation("Order {OrderId} already processed, duplicate skipped", order.Id);
                return;
            }

            try
            {
                bool reserved = await products.ReserveStockAsync(order.Items, async () =>
                {
                    order.MarkProcessed();
                    if (!await orders.TryCreateAsync(order))
                        throw new DuplicateOrderException(order.Id);   // rolls back the stock reduction
                });

                if (!reserved)
                {
                    order.Reject();
                    await orders.TryCreateAsync(order);
                    logger.LogWarning("Order {OrderId} rejected: not enough stock", order.Id);
                }
            }
            catch (DuplicateOrderException)
            {
                logger.LogInformation("Order {OrderId} stored by a parallel duplicate, stock not reduced again", order.Id);
            }
        }
    }
}
