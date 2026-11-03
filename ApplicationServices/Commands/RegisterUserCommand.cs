using MediatR;

namespace ApplicationServices.Commands
{
    public record RegisterUserCommand(string Name, string Email, string Address) : IRequest<string>;
}