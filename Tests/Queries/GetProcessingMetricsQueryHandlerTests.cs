using ApplicationServices.Exceptions;
using ApplicationServices.Queries;
using ApplicationServices.Queries.Handlers;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Orders;
using Moq;

namespace Tests.Queries
{
    public class GetProcessingMetricsQueryHandlerTests
    {
        private static Order ShippedOrder(double hours)
        {
            var order = new Order("cust-1", []);
            order.OrderDate = new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc);
            order.Ship(order.OrderDate.AddHours(hours));
            return order;
        }

        [Test]
        public async Task CalculatesAverage_Fastest_Slowest()
        {
            var repo = new Mock<IReadOrderRepository>();
            repo.Setup(r => r.GetShippedBetweenAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync([ShippedOrder(2), ShippedOrder(4), ShippedOrder(6)]);
            var handler = new GetProcessingMetricsQueryHandler(repo.Object);

            var result = await handler.Handle(
                new GetProcessingMetricsQuery(DateTime.MinValue, DateTime.MaxValue), CancellationToken.None);

            Assert.Multiple(() =>
            {
                Assert.That(result.OrderCount, Is.EqualTo(3));
                Assert.That(result.AverageHours, Is.EqualTo(4).Within(0.001));
                Assert.That(result.FastestHours, Is.EqualTo(2).Within(0.001));
                Assert.That(result.SlowestHours, Is.EqualTo(6).Within(0.001));
            });
        }

        [Test]
        public void FromAfterTo_ThrowsValidation()
        {
            var handler = new GetProcessingMetricsQueryHandler(new Mock<IReadOrderRepository>().Object);

            Assert.ThrowsAsync<ValidationException>(() => handler.Handle(
                new GetProcessingMetricsQuery(new DateTime(2026, 12, 1), new DateTime(2026, 1, 1)), CancellationToken.None));
        }
    }
}