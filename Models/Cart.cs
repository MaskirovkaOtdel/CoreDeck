using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Efstathiadis_Theodors.Models
{
    public class Cart
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? LastModifiedDate { get; set; }

        // Navigation property
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

        public decimal GetTotalPrice()
        {
            return CartItems.Sum(item => item.GetTotalPrice());
        }

        public int GetTotalItems()
        {
            return CartItems.Sum(item => item.Quantity);
        }
    }
}
