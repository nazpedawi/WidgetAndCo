using ApplicationServices.Exceptions;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Products;
using MediatR;

namespace ApplicationServices.Commands.Handlers
{
    public class CreateProductCommandHandler(IWriteProductRepository products)
        : IRequestHandler<CreateProductCommand, string>
    {
        public async Task<string> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Name is required.");
            if (string.IsNullOrWhiteSpace(request.Category))
                throw new ValidationException("Category is required.");
            if (request.Price <= 0)
                throw new ValidationException("Price must be greater than 0.");
            if (request.Stock < 0)
                throw new ValidationException("Stock cannot be negative.");

            var product = new Product
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim() ?? string.Empty,
                Category = request.Category.Trim(),
                Price = request.Price,
                Stock = request.Stock
            };

            await products.AddAsync(product);
            await products.SaveChangesAsync();

            return product.Id;
        }
    }
}