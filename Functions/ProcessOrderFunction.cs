using System.Text.Json;
using ApplicationServices.Commands;
using DataLayer.Repo.Queues;
using DomainLayer.Models.Orders;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Functions
{
    public class ProcessOrderFunction(IMediator mediator, ILogger<ProcessOrderFunction> logger)
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        [Function(nameof(ProcessOrder))]
        public async Task ProcessOrder(
            [QueueTrigger(AzureStorageQueue.OrdersQueue, Connection = "AzureWebJobsStorage")] string message)
        {
            Order? order;
            try
            {
                order = JsonSerializer.Deserialize<Order>(message, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Invalid order message: {Message}", message);
                throw;   // retried, then moved to orders-queue-poison
            }

            if (order is null || string.IsNullOrWhiteSpace(order.Id) || string.IsNullOrWhiteSpace(order.CustomerId))
                throw new InvalidOperationException("Order message is missing id or customerId.");

            await mediator.Send(new ProcessOrderCommand(order));

            logger.LogInformation("Order {OrderId} stored in Cosmos for customer {CustomerId}",
                order.Id, order.CustomerId);
        }
    }
}