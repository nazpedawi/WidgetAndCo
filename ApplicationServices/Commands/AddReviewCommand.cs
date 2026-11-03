using MediatR;

namespace ApplicationServices.Commands
{
    public record AddReviewCommand(string ProductId, int Rating, string Comment) : IRequest<string>;
}