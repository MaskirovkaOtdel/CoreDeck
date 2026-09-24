namespace Final_Efstathiadis_Theodors.Models
{
    public class CompareViewModel
    {
        public const int MaxProducts = 4;

        public List<Product> Products { get; set; } = new();

        public bool HasProducts => Products.Count > 0;
        public bool CanAddMore => Products.Count < MaxProducts;
        public int SlotCount => Products.Count;

        public double GetAverageRating(Product product)
        {
            if (product.Reviews == null || !product.Reviews.Any()) return 0;
            return product.Reviews.Average(r => r.Rating);
        }

        public int GetReviewCount(Product product)
        {
            return product.Reviews?.Count ?? 0;
        }

        public decimal? GetLowestPrice()
        {
            return HasProducts ? Products.Min(p => p.Price) : null;
        }

        public decimal? GetHighestPrice()
        {
            return HasProducts ? Products.Max(p => p.Price) : null;
        }
    }
}
