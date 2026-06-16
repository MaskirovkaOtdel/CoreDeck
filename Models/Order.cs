using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Final_Efstathiadis_Theodors.Models
{
    public enum OrderStatus
    {
        Pending,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Range(0.01, 9999999, ErrorMessage = "Total price must be valid")]
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Order status is required")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [StringLength(500, ErrorMessage = "Shipping address cannot exceed 500 characters")]
        public string ShippingAddress { get; set; } = "";

        [StringLength(20, ErrorMessage = "Shipping postal code cannot exceed 20 characters")]
        public string PostalCode { get; set; } = "";

        [StringLength(100, ErrorMessage = "Shipping city cannot exceed 100 characters")]
        public string City { get; set; } = "";

        [StringLength(100, ErrorMessage = "Shipping country cannot exceed 100 characters")]
        public string Country { get; set; } = "";

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? ShippedDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}", ApplyFormatInEditMode = true)]
        public DateTime? DeliveredDate { get; set; }

        // Navigation property
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
