namespace Final_Efstathiadis_Theodors.Models
{
    public class UserProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<Order> Orders { get; set; } = new();
        public int TotalOrders { get; set; }
        public decimal LifetimeSpent { get; set; }
        public int TotalWishlistItems { get; set; }
        public string? CurrentStatus { get; set; }
        public string? SearchTerm { get; set; }
    }
}
