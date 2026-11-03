using ApplicationServices.DTO;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Users;
using MediatR;

namespace ApplicationServices.Queries.Handlers
{
    public class GetUserByIdQueryHandler(ITableRepository<User> users)
        : IRequestHandler<GetUserByIdQuery, UserDto?>
    {
        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await users.GetAsync(request.UserId, "profile");   // point lookup
            return user is null ? null : UserDto.From(user);
        }
    }
}