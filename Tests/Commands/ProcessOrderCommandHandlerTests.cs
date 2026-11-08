using ApplicationServices.Commands;
using ApplicationServices.Commands.Handlers;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Orders;
using Moq;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Commands
{
    public class ProcessOrderCommandHandlerTests
    {
        private Mock<IWriteOrderRepository> _orders = null!;
        private ProcessOrderCommandHandler _handler = null!;
        private Mock<IWriteProductRepository> _products = null!;

        [SetUp]
        public void SetUp()
        {
            _orders = new Mock<IWriteOrderRepository>();
            _products = new Mock<IWriteProductRepository>();
            _handler = new ProcessOrderCommandHandler(_orders.Object, _products.Object,
                NullLogger<ProcessOrderCommandHandler>.Instance);

            _orders.Setup(r => r.TryCreateAsync(It.IsAny<Order>())).ReturnsAsync(true);
        }

        // Simulates the SQL transaction: runs the save callback only when there is enough stock
        private void StockAvailable(bool available) =>
            _products.Setup(p => p.ReserveStockAsync(It.IsAny<IReadOnlyList<OrderItem>>(), It.IsAny<Func<Task>>()))
                     .Returns<IReadOnlyList<OrderItem>, Func<Task>>(async (_, saveOrder) =>
                     {
                         if (available) await saveOrder();
                         return available;
             });

        [Test]
        public async Task NewOrder_WithStock_IsStored_AsProcessed()
        {
            var order = new Order("cust-1", []);
            StockAvailable(true);

            await _handler.Handle(new ProcessOrderCommand(order), CancellationToken.None);

            _orders.Verify(r => r.TryCreateAsync(It.Is<Order>(o => o.Status == OrderStatus.Processed)), Times.Once);
        }

        [Test]
        public async Task NewOrder_WithoutStock_IsStored_AsRejected()
        {
            var order = new Order("cust-1", []);
            StockAvailable(false);

            await _handler.Handle(new ProcessOrderCommand(order), CancellationToken.None);

            _orders.Verify(r => r.TryCreateAsync(It.Is<Order>(o => o.Status == OrderStatus.Rejected)), Times.Once);
        }

        [Test]
        public void ParallelDuplicate_DetectedByCosmosConflict_IsHandledWithoutError()
        {
            var order = new Order("cust-1", []);
            StockAvailable(true);
            _orders.Setup(r => r.TryCreateAsync(It.IsAny<Order>())).ReturnsAsync(false);   // 409

            Assert.DoesNotThrowAsync(() => _handler.Handle(new ProcessOrderCommand(order), CancellationToken.None));
        }

        [Test]
        public async Task DuplicateMessage_IsSkipped()
        {
            var order = new Order("cust-1", []);
            _orders.Setup(r => r.GetByIdAsync(order.Id, "cust-1")).ReturnsAsync(order);   // already stored

            await _handler.Handle(new ProcessOrderCommand(order), CancellationToken.None);

             _orders.Verify(r => r.TryCreateAsync(It.IsAny<Order>()), Times.Never);
            _products.Verify(p => p.ReserveStockAsync(It.IsAny<IReadOnlyList<OrderItem>>(), It.IsAny<Func<Task>>()), Times.Never);
        }
    }
}