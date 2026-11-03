using ApplicationServices.DTO;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Reviews;
using MediatR;

namespace ApplicationServices.Queries.Handlers
{
    public class GetReviewsForProductQueryHandler(ITableRepository<Review> reviews)
        : IRequestHandler<GetReviewsForProductQuery, IEnumerable<ReviewDto>>
    {
        public async Task<IEnumerable<ReviewDto>> Handle(GetReviewsForProductQuery request, CancellationToken cancellationToken)
        {
            int max = Math.Clamp(request.Max, 1, 100);
            var result = await reviews.GetByPartitionKeyAsync(request.ProductId, max);
            return result.Select(ReviewDto.From);
        }
    }
}