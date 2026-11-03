using MediatR;

namespace ApplicationServices.Commands
{
    public record CreateProductCommand(string Name, string Description, string Category,
                                       decimal Price, int Stock) : IRequest<string>;
}