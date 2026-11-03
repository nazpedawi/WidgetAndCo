using ApplicationServices.Exceptions;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Reviews;
using MediatR;

namespace ApplicationServices.Commands.Handlers
{
    public class AddReviewCommandHandler(IReadProductRepository products, ITableRepository<Review> reviews)
        : IRequestHandler<AddReviewCommand, string>
    {
        public async Task<string> Handle(AddReviewCommand request, CancellationToken cancellationToken)
        {
            if (request.Rating is < 1 or > 5)
                throw new ValidationException("Rating must be between 1 and 5.");
            if (string.IsNullOrWhiteSpace(request.Comment) || request.Comment.Length > 1000)
                throw new ValidationException("Comment is required and may be at most 1000 characters.");

            if (await products.GetByIdAsync(request.ProductId) is null)
                throw new NotFoundException($"Product '{request.ProductId}' not found.");

            var review = new Review(request.ProductId, request.Rating, request.Comment.Trim());
            await reviews.InsertOrUpdateAsync(review);

            return review.RowKey;
        }
    }
}