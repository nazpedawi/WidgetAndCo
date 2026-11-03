using DomainLayer.Models.Orders;

namespace ApplicationServices.DTO
{
    public record OrderDto(string Id, string CustomerId, DateTime OrderDate, DateTime? ShippedDate,
                           string Status, decimal TotalAmount, double? ProcessingHours, List<OrderItem> Items)
    {
        public static OrderDto From(Order o) => new(
            o.Id, o.CustomerId, o.OrderDate, o.ShippedDate, o.Status.ToString(),
            o.TotalAmount, o.GetProcessingTime()?.TotalHours, o.Items);
    }
}