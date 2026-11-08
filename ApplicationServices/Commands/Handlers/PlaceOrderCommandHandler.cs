using ApplicationServices.Exceptions;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Orders;
using DomainLayer.Models.Users;
using MediatR;

namespace ApplicationServices.Commands.Handlers
{
    public class PlaceOrderCommandHandler(
        IReadProductRepository products,
        ITableRepository<User> users,
        IAzureStorageQueue queue)
        : IRequestHandler<PlaceOrderCommand, string>
    {
        public async Task<string> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
        {
            // Business rules / validation
            if (string.IsNullOrWhiteSpace(request.CustomerId))
                throw new ValidationException("CustomerId is required.");
            if (request.Items is null || request.Items.Count == 0)
                throw new ValidationException("An order needs at least one item.");
            if (request.Items.Any(i => i.Quantity <= 0))
                throw new ValidationException("Quantity must be at least 1.");
            if (request.OrderId is not null && !Guid.TryParse(request.OrderId, out _))
                throw new ValidationException("OrderId must be a valid GUID.");

            // Customer must exist in the shared Users table
            if (await users.GetAsync(request.CustomerId, "profile") is null)
                throw new NotFoundException($"Customer '{request.CustomerId}' not found.");

            var items = new List<OrderItem>();
            foreach (var line in request.Items)
            {
                var product = await products.GetByIdAsync(line.ProductId)
                    ?? throw new NotFoundException($"Product '{line.ProductId}' not found.");

                    if (product.Stock < line.Quantity)
                    throw new ValidationException($"Not enough stock for '{product.Name}'.");

                items.Add(new OrderItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = line.Quantity,
                    UnitPrice = product.Price
                });
            }

            var order = new Order(request.CustomerId, items);

            // Idempotency key: a double submit with the same id is stored only once
            if (request.OrderId is not null)
                order.Id = request.OrderId;

            // Async: the API returns immediately, the Function writes to Cosmos (load levelling)
            await queue.InsertMessageAsync(order);

            return order.Id;
        }
    }
}