using ApplicationServices.DTO;
using MediatR;

namespace ApplicationServices.Queries
{
    public record GetReviewsForProductQuery(string ProductId, int Max = 10) : IRequest<IEnumerable<ReviewDto>>;
}