using System.ComponentModel.DataAnnotations;

namespace ProductManagement.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Range(0, 9999999999.99)]
        public decimal Price { get; set; }

        public IFormFile[] NewImages { get; set; }


        public List<ProductImageDto> ExistingImages { get; set; } = new List<ProductImageDto>();
    }
    public class ProductImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
    }
}
