using ApplicationServices.DTO;
using MediatR;

namespace ApplicationServices.Queries
{
    public record GetUserByIdQuery(string UserId) : IRequest<UserDto?>;
}