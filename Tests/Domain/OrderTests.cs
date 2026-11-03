using DomainLayer.Events;
using DomainLayer.Models.Orders;

namespace Tests.Domain
{
    public class OrderTests
    {
        private static Order NewOrder() => new("cust-1",
            [new OrderItem { ProductId = "p1", ProductName = "Widget", Quantity = 2, UnitPrice = 10m }]);

        [Test]
        public void NewOrder_HasStatusPlaced_AndCorrectTotal()
        {
            var order = NewOrder();

            Assert.Multiple(() =>
            {
                Assert.That(order.Status, Is.EqualTo(OrderStatus.Placed));
                Assert.That(order.TotalAmount, Is.EqualTo(20m));
                Assert.That(order.ShippedDate, Is.Null);
            });
        }

        [Test]
        public void Ship_SetsShippedDate_AndCalculatesProcessingTime()
        {
            var order = NewOrder();
            order.OrderDate = new DateTime(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);

            order.Ship(new DateTime(2026, 11, 1, 13, 0, 0, DateTimeKind.Utc));

            Assert.Multiple(() =>
            {
                Assert.That(order.Status, Is.EqualTo(OrderStatus.Shipped));
                Assert.That(order.GetProcessingTime(), Is.EqualTo(TimeSpan.FromHours(3)));
            });
        }

        [Test]
        public void Ship_RaisesOrderShippedEvent()
        {
            var order = NewOrder();

            order.Ship(DateTime.UtcNow);

            Assert.That(order.GetDomainEvents(), Has.One.TypeOf<OrderShippedEvent>());
        }

        [Test]
        public void Ship_Twice_Throws()
        {
            var order = NewOrder();
            order.Ship(DateTime.UtcNow);

            Assert.Throws<InvalidOperationException>(() => order.Ship(DateTime.UtcNow));
        }

        [Test]
        public void Ship_BeforeOrderDate_Throws()
        {
            var order = NewOrder();

            Assert.Throws<ArgumentException>(() => order.Ship(order.OrderDate.AddHours(-1)));
        }
    }
}