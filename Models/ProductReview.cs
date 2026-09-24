using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Efstathiadis_Theodors.Models
{
    public class ProductReview
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5 stars")]
        public int Rating { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = "";

        [Required(ErrorMessage = "Review comment is required")]
        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        public string Comment { get; set; } = "";

        public bool IsVerifiedBuyer { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
