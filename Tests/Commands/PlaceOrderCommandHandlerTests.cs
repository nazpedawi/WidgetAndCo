using ApplicationServices.Commands;
using ApplicationServices.Commands.Handlers;
using ApplicationServices.DTO;
using ApplicationServices.Exceptions;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Orders;
using DomainLayer.Models.Products;
using DomainLayer.Models.Users;
using Moq;

namespace Tests.Commands
{
    public class PlaceOrderCommandHandlerTests
    {
        private Mock<IReadProductRepository> _products = null!;
        private Mock<ITableRepository<User>> _users = null!;
        private Mock<IAzureStorageQueue> _queue = null!;
        private PlaceOrderCommandHandler _handler = null!;

        [SetUp]
        public void SetUp()
        {
            _products = new Mock<IReadProductRepository>();
            _users = new Mock<ITableRepository<User>>();
            _queue = new Mock<IAzureStorageQueue>();
            _handler = new PlaceOrderCommandHandler(_products.Object, _users.Object, _queue.Object);

            // Default: customer and product exist
            _users.Setup(u => u.GetAsync("cust-1", "profile"))
                  .ReturnsAsync(new User("Jane", "jane@example.com", "Street 1"));
            _products.Setup(p => p.GetByIdAsync("prod-1"))
                     .ReturnsAsync(new Product { Id = "prod-1", Name = "Blue Widget", Category = "Widgets", Price = 19.99m });
        }

        [Test]
        public async Task ValidOrder_IsQueued_WithPriceFromDatabase()
        {
            Order? queued = null;
            _queue.Setup(q => q.InsertMessageAsync(It.IsAny<Order>()))
                  .Callback<Order>(o => queued = o)
                  .Returns(Task.CompletedTask);

            string orderId = await _handler.Handle(
                new PlaceOrderCommand("cust-1", [new OrderLineDto("prod-1", 2)]), CancellationToken.None);

            _queue.Verify(q => q.InsertMessageAsync(It.IsAny<Order>()), Times.Once);
            Assert.Multiple(() =>
            {
                Assert.That(queued, Is.Not.Null);
                Assert.That(queued!.Id, Is.EqualTo(orderId));
                Assert.That(queued.Items[0].UnitPrice, Is.EqualTo(19.99m));   // not from the client
                Assert.That(queued.TotalAmount, Is.EqualTo(39.98m));
            });
        }

        [Test]
        public void EmptyOrder_ThrowsValidation_AndNothingIsQueued()
        {
            Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(new PlaceOrderCommand("cust-1", []), CancellationToken.None));

            _queue.Verify(q => q.InsertMessageAsync(It.IsAny<Order>()), Times.Never);
        }

        [Test]
        public void ZeroQuantity_ThrowsValidation()
        {
            Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(new PlaceOrderCommand("cust-1", [new OrderLineDto("prod-1", 0)]), CancellationToken.None));
        }

        [Test]
        public void UnknownCustomer_ThrowsNotFound()
        {
            Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(new PlaceOrderCommand("nobody", [new OrderLineDto("prod-1", 1)]), CancellationToken.None));

            _queue.Verify(q => q.InsertMessageAsync(It.IsAny<Order>()), Times.Never);
        }

        [Test]
        public void UnknownProduct_ThrowsNotFound()
        {
            Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(new PlaceOrderCommand("cust-1", [new OrderLineDto("missing", 1)]), CancellationToken.None));
        }
    }
}