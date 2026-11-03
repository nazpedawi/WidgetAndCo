using ApplicationServices.Extensions;
using DataLayer.Context;
using DataLayer.Extensions;
using Microsoft.EntityFrameworkCore;
using WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

string Conn(string name) => builder.Configuration.GetConnectionString(name)
    ?? throw new InvalidOperationException($"Connection string '{name}' is missing.");

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

// Business tier + data tier
builder.Services.AddApplicationServices();
builder.Services.AddCosmosDb(Conn("CosmosDb"));
builder.Services.AddSqlDb(Conn("Sql"));
builder.Services.AddAzureStorage(Conn("AzureStorage"));

var app = builder.Build();

// Create databases, containers, tables and queue if they don't exist
using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<ProductDbContext>().Database.MigrateAsync();
}
await app.Services.InitializeCosmosAsync();
await app.Services.InitializeStorageAsync();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();