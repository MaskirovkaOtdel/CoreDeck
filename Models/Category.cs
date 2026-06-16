using System.ComponentModel.DataAnnotations;

namespace Final_Efstathiadis_Theodors.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
        public string Name { get; set; } = "";

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string Description { get; set; } = "";

        [StringLength(255, ErrorMessage = "Icon URL cannot exceed 255 characters")]
        public string IconUrl { get; set; } = "";

        // Navigation property
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
