using DomainLayer.Events;

namespace DomainLayer.Models.Orders
{
    public class Order : BaseEntity
    {
        public string CustomerId { get; set; } = string.Empty;   // Cosmos partition key
        public DateTime OrderDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItem> Items { get; set; } = new();

        public decimal TotalAmount => Items.Sum(i => i.UnitPrice * i.Quantity);

        public Order() { }   // needed for deserialisation

        public Order(string customerId, List<OrderItem> items)
        {
            CustomerId = customerId;
            Items = items;
            OrderDate = DateTime.UtcNow;
            Status = OrderStatus.Placed;
        }

        public void MarkProcessed() => Status = OrderStatus.Processed;

        public void Ship(DateTime shippedDate)
        {
            if (Status == OrderStatus.Shipped)
                throw new InvalidOperationException("Order is already shipped.");
            if (shippedDate < OrderDate)
                throw new ArgumentException("Shipping date cannot be before the order date.");

            ShippedDate = shippedDate;
            Status = OrderStatus.Shipped;
            AddDomainEvent(new OrderShippedEvent(Id, CustomerId, GetProcessingTime()!.Value));
        }

        // The "orderprocessed" metric from the scenario
        public TimeSpan? GetProcessingTime() => ShippedDate - OrderDate;
    }
}