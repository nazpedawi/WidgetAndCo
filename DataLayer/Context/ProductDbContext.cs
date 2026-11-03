using DomainLayer.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Context
{
    public class ProductDbContext : DbContext
    {
        public DbSet<Product> Products => Set<Product>();

        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Product>(p =>
            {
                p.HasKey(x => x.Id);
                p.Property(x => x.Id).HasMaxLength(36);
                p.Property(x => x.Name).HasMaxLength(200).IsRequired();
                p.Property(x => x.Category).HasMaxLength(100).IsRequired();
                p.Property(x => x.Price).HasPrecision(18, 2);
                p.Property(x => x.ImageBlobName).HasMaxLength(300);

                p.HasIndex(x => new { x.Category, x.Price });
            });
        }
    }
}