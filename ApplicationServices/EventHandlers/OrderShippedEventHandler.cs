using DomainLayer.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ApplicationServices.EventHandlers
{
    public class OrderShippedEventHandler(ILogger<OrderShippedEventHandler> logger)
        : INotificationHandler<OrderShippedEvent>
    {
        public Task Handle(OrderShippedEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("Order {OrderId} shipped after {Hours:F1} hours",
                notification.OrderId, notification.ProcessingTime.TotalHours);
            return Task.CompletedTask;
        }
    }
}