using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Range(0, 9999999999.99)]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    }
}
