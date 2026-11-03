using ApplicationServices.Commands;
using ApplicationServices.Commands.Handlers;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Orders;
using Moq;

namespace Tests.Commands
{
    public class ProcessOrderCommandHandlerTests
    {
        private Mock<IWriteOrderRepository> _orders = null!;
        private ProcessOrderCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _orders = new Mock<IWriteOrderRepository>();
            _handler = new ProcessOrderCommandHandler(_orders.Object);
        }

        [Test]
        public async Task NewOrder_IsSaved_AsProcessed()
        {
            var order = new Order("cust-1", []);
            _orders.Setup(r => r.GetByIdAsync(order.Id, "cust-1")).ReturnsAsync((Order?)null);

            await _handler.Handle(new ProcessOrderCommand(order), CancellationToken.None);

            _orders.Verify(r => r.SaveAsync(It.Is<Order>(o => o.Status == OrderStatus.Processed)), Times.Once);
        }

        [Test]
        public async Task DuplicateMessage_IsSkipped()
        {
            var order = new Order("cust-1", []);
            _orders.Setup(r => r.GetByIdAsync(order.Id, "cust-1")).ReturnsAsync(order);   // already stored

            await _handler.Handle(new ProcessOrderCommand(order), CancellationToken.None);

            _orders.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Never);
        }
    }
}