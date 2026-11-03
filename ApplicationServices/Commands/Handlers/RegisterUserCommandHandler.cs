using ApplicationServices.Exceptions;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Users;
using MediatR;

namespace ApplicationServices.Commands.Handlers
{
    public class RegisterUserCommandHandler(ITableRepository<User> users)
        : IRequestHandler<RegisterUserCommand, string>
    {
        public async Task<string> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Name is required.");
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
                throw new ValidationException("A valid email is required.");

            var user = new User(request.Name.Trim(), request.Email.Trim(), request.Address?.Trim() ?? string.Empty);
            await users.InsertOrUpdateAsync(user);

            return user.PartitionKey;   // the userId
        }
    }
}