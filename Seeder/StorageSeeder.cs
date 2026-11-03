using Azure.Data.Tables;
using DataLayer.Context;
using DataLayer.Repo.Queues;
using DataLayer.Repo.Tables;
using DomainLayer.Models.Orders;
using DomainLayer.Models.Products;
using DomainLayer.Models.Reviews;
using DomainLayer.Models.Users;
using Microsoft.EntityFrameworkCore;

// Fills SQL, Table Storage and the queue with realistic demo data
public static class StorageSeeder
{
    private const string StorageConnection = "UseDevelopmentStorage=true";
    private const string SqlConnection =
        "Server=(localdb)\\MSSQLLocalDB;Database=WidgetAndCo;Trusted_Connection=True;TrustServerCertificate=True;";

    private const int UserCount = 50;
    private const int QueuedOrderCount = 15;

    public static async Task RunAsync()
    {
        var random = new Random(7);

        // ---------- 1. Products (SQL) ----------
        var dbOptions = new DbContextOptionsBuilder<ProductDbContext>().UseSqlServer(SqlConnection).Options;
        await using var db = new ProductDbContext(dbOptions);

        string[] colours = ["Blue", "Red", "Green", "Black"];
        (string Name, string Category, decimal BasePrice)[] types =
        [
            ("Widget", "Widgets", 19.99m),
            ("Gadget", "Gadgets", 34.99m),
            ("Widget Holder", "Accessories", 9.99m)
        ];

        var products = new List<Product>();
        foreach (var type in types)
        foreach (var colour in colours)
        {
            products.Add(new Product
            {
                Name = $"{colour} {type.Name}",
                Description = $"A high-quality {colour.ToLower()} {type.Name.ToLower()} from Widget and Co.",
                Category = type.Category,
                Price = type.BasePrice + random.Next(0, 10),
                Stock = random.Next(10, 500)
            });
        }

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
        Console.WriteLine($"SQL      : {products.Count} products added");

        // ---------- 2. Users (Table Storage, PartitionKey = userId) ----------
        var tableService = new TableServiceClient(StorageConnection);
        TableClient usersTable = tableService.GetTableClient(TableRepository<User>.TableNameFor<User>());
        await usersTable.CreateIfNotExistsAsync();

        string[] firstNames = ["Emma", "Daan", "Sophie", "Lucas", "Julia", "Sem", "Tess", "Finn", "Anna", "Noah",
                               "Lisa", "Milan", "Sara", "Levi", "Eva", "Bram", "Fleur", "Thijs", "Iris", "Jesse"];
        string[] lastNames = ["de Jong", "Jansen", "de Vries", "van den Berg", "Bakker", "Visser", "Smit",
                              "Meijer", "de Boer", "Mulder", "Bos", "Vos", "Peters", "Hendriks"];
        string[] cities = ["Amsterdam", "Haarlem", "Alkmaar", "Zaandam", "Hoorn", "Den Helder", "Purmerend", "Hilversum"];
        string[] streets = ["Dorpsstraat", "Kerkstraat", "Molenweg", "Stationsplein", "Schoolstraat", "Lindenlaan"];

        var users = new List<User>();
        for (int i = 0; i < UserCount; i++)
        {
            string first = firstNames[random.Next(firstNames.Length)];
            string last = lastNames[random.Next(lastNames.Length)];
            string email = $"{first}.{last.Replace(" ", "")}{random.Next(10, 99)}@example.com".ToLower();
            string address = $"{streets[random.Next(streets.Length)]} {random.Next(1, 200)}, {cities[random.Next(cities.Length)]}";

            var user = new User($"{first} {last}", email, address);
            await usersTable.UpsertEntityAsync(user);
            users.Add(user);
        }
        Console.WriteLine($"Tables   : {users.Count} users added to 'Users'");

        // ---------- 3. Reviews (Table Storage, PartitionKey = productId) ----------
        TableClient reviewsTable = tableService.GetTableClient(TableRepository<Review>.TableNameFor<Review>());
        await reviewsTable.CreateIfNotExistsAsync();

        string[][] commentsByRating =
        [
            ["Broke after one week.", "Not as described, very disappointed."],
            ["Below expectations.", "Works, but feels cheap."],
            ["Does the job.", "Average quality for the price."],
            ["Good value, would buy again.", "Solid product, fast delivery.", "Happy with it."],
            ["Best widget I ever bought!", "Excellent quality, highly recommended.", "Perfect, exactly what I needed."]
        ];

        int reviewCount = 0;
        foreach (var product in products)
        {
            // All reviews of one product share a partition, so they can be written as ONE batch transaction
            var batch = new List<TableTransactionAction>();
            int count = random.Next(3, 9);

            for (int i = 0; i < count; i++)
            {
                int rating = random.NextDouble() < 0.7 ? random.Next(4, 6) : random.Next(1, 4);   // mostly positive
                string[] options = commentsByRating[rating - 1];

                var review = new Review(product.Id, rating, options[random.Next(options.Length)]);
                review.CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 60)).AddMinutes(-random.Next(0, 1440));
                review.RowKey = $"{DateTime.MaxValue.Ticks - review.CreatedAt.Ticks:D19}_{Guid.NewGuid()}";

                batch.Add(new TableTransactionAction(TableTransactionActionType.Add, review));
            }

            await reviewsTable.SubmitTransactionAsync(batch);
            reviewCount += batch.Count;
        }
        Console.WriteLine($"Tables   : {reviewCount} reviews added to 'Reviews' ({products.Count} partitions, one batch each)");

        // ---------- 4. Orders waiting on the queue ----------
        var queue = new AzureStorageQueue(StorageConnection, AzureStorageQueue.OrdersQueue);
        await queue.Client.CreateIfNotExistsAsync();

        for (int i = 0; i < QueuedOrderCount; i++)
        {
            User customer = users[random.Next(users.Count)];

            var items = products
                .OrderBy(_ => random.Next())
                .Take(random.Next(1, 4))
                .Select(p => new OrderItem
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    Quantity = random.Next(1, 4),
                    UnitPrice = p.Price
                })
                .ToList();

            await queue.InsertMessageAsync(new Order(customer.PartitionKey, items));
        }
        Console.WriteLine($"Queue    : {QueuedOrderCount} orders waiting on '{AzureStorageQueue.OrdersQueue}'");
    }
}