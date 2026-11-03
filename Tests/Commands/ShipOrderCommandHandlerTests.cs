using ApplicationServices.Commands;
using ApplicationServices.Commands.Handlers;
using ApplicationServices.Exceptions;
using DataLayer.Repo.Interfaces;
using DomainLayer.Events;
using DomainLayer.Models.Orders;
using MediatR;
using Moq;

namespace Tests.Commands
{
    public class ShipOrderCommandHandlerTests
    {
        private Mock<IWriteOrderRepository> _orders = null!;
        private Mock<IPublisher> _publisher = null!;
        private ShipOrderCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _orders = new Mock<IWriteOrderRepository>();
            _publisher = new Mock<IPublisher>();
            _handler = new ShipOrderCommandHandler(_orders.Object, _publisher.Object);
        }

        [Test]
        public async Task ExistingOrder_IsShipped_SavedAndEventPublished()
        {
            var order = new Order("cust-1", []);
            _orders.Setup(r => r.GetByIdAsync(order.Id, "cust-1")).ReturnsAsync(order);

            var result = await _handler.Handle(new ShipOrderCommand(order.Id, "cust-1"), CancellationToken.None);

            Assert.That(result.Status, Is.EqualTo("Shipped"));
            _orders.Verify(r => r.SaveAsync(order), Times.Once);
            _publisher.Verify(p => p.Publish(
                It.Is<INotification>(n => n is OrderShippedEvent), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public void MissingOrder_ThrowsNotFound()
        {
            _orders.Setup(r => r.GetByIdAsync("x", "cust-1")).ReturnsAsync((Order?)null);

            Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(new ShipOrderCommand("x", "cust-1"), CancellationToken.None));
        }

        [Test]
        public void AlreadyShipped_ThrowsValidation_AndIsNotSavedAgain()
        {
            var order = new Order("cust-1", []);
            order.Ship(DateTime.UtcNow);
            _orders.Setup(r => r.GetByIdAsync(order.Id, "cust-1")).ReturnsAsync(order);

            Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(new ShipOrderCommand(order.Id, "cust-1"), CancellationToken.None));

            _orders.Verify(r => r.SaveAsync(It.IsAny<Order>()), Times.Never);
        }
    }
}