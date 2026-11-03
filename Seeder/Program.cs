using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using DataLayer.Extensions;
using DomainLayer.Models.Orders;
using Microsoft.Azure.Cosmos;

// "dotnet run --project Seeder -- storage" seeds SQL, Tables and the queue instead of Cosmos
if (args.Contains("storage"))
{
    await StorageSeeder.RunAsync();
    return;
}

// Local emulator only (public, well-known key) - same as appsettings.json
const string ConnectionString =
    "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
const int CustomerCount = 200;
const int OrderCount = 1000;
const int MaxParallelInserts = 20;

var random = new Random(42);   // fixed seed: same data every run, so measurements are reproducible
string[] products = ["Blue Widget", "Red Widget", "Green Widget", "Widget Pro", "Widget Mini", "Widget XL"];

// Same serializer settings as the app (camelCase, so Id becomes "id").
// No bulk mode: we limit concurrency ourselves, like a queue consumer instead of a flood.
using var client = new CosmosClient(ConnectionString, new CosmosClientOptions
{
    AllowBulkExecution = false,
    MaxRetryAttemptsOnRateLimitedRequests = 50,
    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromMinutes(2),
    SerializerOptions = new CosmosSerializationOptions { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
});

Container container = client.GetContainer(CosmosExtensions.DatabaseName, CosmosExtensions.OrdersContainer);

try
{
    await container.ReadContainerAsync();
}
catch (CosmosException)
{
    Console.WriteLine("Orders container not found. Start the WebApi once first so it creates the container.");
    return;
}

// ---------- 1. Seed ----------
List<Order> orders = GenerateOrders();
Console.WriteLine($"Seeding {OrderCount} orders for {CustomerCount} customers " +
                  $"(max {MaxParallelInserts} inserts in parallel)...");

var charges = new ConcurrentBag<double>();
int throttled = 0;
using var gate = new SemaphoreSlim(MaxParallelInserts);
var stopwatch = Stopwatch.StartNew();

await Task.WhenAll(orders.Select(async order =>
{
    await gate.WaitAsync();
    try
    {
        while (true)
        {
            try
            {
                ItemResponse<Order> response = await container.CreateItemAsync(order, new PartitionKey(order.CustomerId));
                charges.Add(response.RequestCharge);
                break;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // 429: RU/s budget exceeded. Wait as long as Cosmos tells us (retry-after), then retry.
                Interlocked.Increment(ref throttled);
                await Task.Delay(ex.RetryAfter ?? TimeSpan.FromMilliseconds(500));
            }
        }
    }
    finally
    {
        gate.Release();
    }
}));

stopwatch.Stop();
Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds} ms. Total {charges.Sum():F0} RU, " +
                  $"average {charges.Average():F2} RU per insert, {throttled} throttled (429) retries.\n");

// ---------- 2. Measure ----------
Order sample = orders[0];
var results = new List<(string Operation, double RU, int Items)>();

ItemResponse<Order> pointRead = await container.ReadItemAsync<Order>(sample.Id, new PartitionKey(sample.CustomerId));
results.Add(("Point read (id + partition key)", pointRead.RequestCharge, 1));

results.Add(await Query("Single-partition query (one customer)",
    new QueryDefinition("SELECT * FROM o WHERE o.customerId = @c").WithParameter("@c", sample.CustomerId),
    partitionKey: sample.CustomerId));

results.Add(await Query("Cross-partition query (totalAmount > 300)",
    new QueryDefinition("SELECT * FROM o WHERE o.totalAmount > @min").WithParameter("@min", 300),
    partitionKey: null));

results.Add(await Query("Cross-partition metrics query (last 7 days)",
    new QueryDefinition("SELECT * FROM o WHERE o.shippedDate >= @from AND o.shippedDate <= @to")
        .WithParameter("@from", DateTime.UtcNow.AddDays(-7))
        .WithParameter("@to", DateTime.UtcNow),
    partitionKey: null));

Console.WriteLine($"{"Operation",-45} {"RU",10} {"Items",8}");
Console.WriteLine(new string('-', 65));
foreach (var (operation, ru, items) in results)
    Console.WriteLine($"{operation,-45} {ru,10:F2} {items,8}");

// ---------- helpers ----------
async Task<(string, double, int)> Query(string name, QueryDefinition query, string? partitionKey)
{
    var options = new QueryRequestOptions
    {
        // With a partition key the query stays in one partition; without it, it fans out
        PartitionKey = partitionKey is null ? null : new PartitionKey(partitionKey)
    };

    double ru = 0;
    int count = 0;
    using FeedIterator<Order> iterator = container.GetItemQueryIterator<Order>(query, requestOptions: options);
    while (iterator.HasMoreResults)
    {
        FeedResponse<Order> page = await iterator.ReadNextAsync();
        ru += page.RequestCharge;
        count += page.Count;
    }
    return (name, ru, count);
}

List<Order> GenerateOrders()
{
    var list = new List<Order>(OrderCount);
    for (int i = 0; i < OrderCount; i++)
    {
        // Spread evenly over many customers = many logical partitions, no hot partition
        string customerId = $"seed-cust-{random.Next(CustomerCount):D3}";

        var items = Enumerable.Range(0, random.Next(1, 5)).Select(_ => new OrderItem
        {
            ProductId = Guid.NewGuid().ToString(),
            ProductName = products[random.Next(products.Length)],
            Quantity = random.Next(1, 5),
            UnitPrice = Math.Round((decimal)(random.NextDouble() * 95 + 5), 2)
        }).ToList();

        var order = new Order(customerId, items)
        {
            OrderDate = DateTime.UtcNow.AddDays(-random.Next(0, 30)).AddMinutes(-random.Next(0, 1440))
        };
        order.MarkProcessed();

        // ~70% shipped, processing time between 1 and 72 hours
        if (random.NextDouble() < 0.7)
        {
            DateTime shipped = order.OrderDate.AddHours(random.Next(1, 73));
            if (shipped <= DateTime.UtcNow)
                order.Ship(shipped);
        }

        list.Add(order);
    }
    return list;
}