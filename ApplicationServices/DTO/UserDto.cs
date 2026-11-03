using DomainLayer.Models.Users;

namespace ApplicationServices.DTO
{
    public record UserDto(string Id, string Name, string Email, string Address)
    {
        public static UserDto From(User u) => new(u.PartitionKey, u.Name, u.Email, u.Address);
    }
}