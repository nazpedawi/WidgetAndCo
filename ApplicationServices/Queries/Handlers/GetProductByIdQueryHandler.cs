using ApplicationServices.DTO;
using DataLayer.Repo.Interfaces;
using MediatR;

namespace ApplicationServices.Queries.Handlers
{
    public class GetProductByIdQueryHandler(IReadProductRepository products, IImageStorage images)
        : IRequestHandler<GetProductByIdQuery, ProductDto?>
    {
        public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await products.GetByIdAsync(request.ProductId);
            if (product is null) return null;

            string? imageUrl = product.ImageBlobName is null
                ? null
                : images.GetReadUrl(product.ImageBlobName, TimeSpan.FromHours(1)).ToString();

            return ProductDto.From(product, imageUrl);
        }
    }
}