using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProductManagement.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required, StringLength(500)]
        public string ImageUrl { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }
    }
}
