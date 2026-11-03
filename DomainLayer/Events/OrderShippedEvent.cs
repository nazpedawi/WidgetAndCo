using MediatR;

namespace DomainLayer.Events
{
    public record OrderShippedEvent(string OrderId, string CustomerId, TimeSpan ProcessingTime) : INotification;
}