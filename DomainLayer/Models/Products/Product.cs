namespace DomainLayer.Models.Products
{
    public class Product : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string? ImageBlobName { get; set; }   // only the blob name; the SAS link is made on read
    }
}