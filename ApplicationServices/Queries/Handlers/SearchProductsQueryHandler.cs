using ApplicationServices.DTO;
using ApplicationServices.Exceptions;
using DataLayer.Repo.Interfaces;
using MediatR;

namespace ApplicationServices.Queries.Handlers
{
    public class SearchProductsQueryHandler(IReadProductRepository products, IImageStorage images)
        : IRequestHandler<SearchProductsQuery, IEnumerable<ProductDto>>
    {
        public async Task<IEnumerable<ProductDto>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
        {
            if (request.MinPrice > request.MaxPrice)
                throw new ValidationException("MinPrice cannot be greater than MaxPrice.");

            var result = await products.SearchAsync(request.Category, request.MinPrice, request.MaxPrice);

            return result.Select(p => ProductDto.From(p,
                p.ImageBlobName is null ? null : images.GetReadUrl(p.ImageBlobName, TimeSpan.FromHours(1)).ToString()));
        }
    }
}