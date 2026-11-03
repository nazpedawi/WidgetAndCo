using DomainLayer.Models.Reviews;

namespace ApplicationServices.DTO
{
    public record ReviewDto(int Rating, string Comment, DateTime CreatedAt)
    {
        public static ReviewDto From(Review r) => new(r.Rating, r.Comment, r.CreatedAt);
    }
}