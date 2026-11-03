using ApplicationServices.DTO;
using MediatR;

namespace ApplicationServices.Queries
{
    public record GetProductByIdQuery(string ProductId) : IRequest<ProductDto?>;
}