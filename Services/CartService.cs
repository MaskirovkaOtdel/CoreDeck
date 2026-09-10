using Microsoft.EntityFrameworkCore;
using Final_Efstathiadis_Theodors.Data;
using Final_Efstathiadis_Theodors.Models;

namespace Final_Efstathiadis_Theodors.Services
{
    public interface ICartService
    {
        Task<Cart> GetOrCreateCartAsync(string userId);
        Task<bool> AddToCartAsync(string userId, int productId, int quantity = 1);
        Task<bool> RemoveFromCartAsync(string userId, int cartItemId);
        Task<bool> UpdateQuantityAsync(string userId, int cartItemId, int quantity);
        Task<decimal> GetCartTotalAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
        Task ClearCartAsync(string userId);
    }

    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetOrCreateCartAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedDate = DateTime.UtcNow };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
                cart.CartItems = new List<CartItem>();
            }

            return cart;
        }

        public async Task<bool> AddToCartAsync(string userId, int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive || quantity <= 0 || product.Stock <= 0)
                return false;

            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (existingItem != null)
            {
                var newQuantity = existingItem.Quantity + quantity;
                if (newQuantity > product.Stock)
                    newQuantity = product.Stock;

                existingItem.Quantity = newQuantity;
                existingItem.UnitPrice = product.Price;
            }
            else
            {
                var addQuantity = Math.Min(quantity, product.Stock);
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = addQuantity,
                    UnitPrice = product.Price,
                    AddedDate = DateTime.UtcNow
                };
                cart.CartItems.Add(cartItem);
            }

            cart.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
                return false;

            var cart = await _context.Carts.FindAsync(cartItem.CartId);
            if (cart?.UserId != userId)
                return false;

            _context.CartItems.Remove(cartItem);
            if (cart != null)
                cart.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateQuantityAsync(string userId, int cartItemId, int quantity)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.Product)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId);

            if (cartItem == null)
                return false;

            var cart = await _context.Carts.FindAsync(cartItem.CartId);
            if (cart?.UserId != userId)
                return false;

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                var maxStock = cartItem.Product?.Stock ?? quantity;
                cartItem.Quantity = Math.Min(quantity, maxStock);
                if (cartItem.Product != null)
                {
                    cartItem.UnitPrice = cartItem.Product.Price;
                }
            }

            if (cart != null)
                cart.LastModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetCartTotalAsync(string userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return cart.GetTotalPrice();
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                return 0;

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return cart?.GetTotalItems() ?? 0;
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            _context.CartItems.RemoveRange(cart.CartItems);
            cart.LastModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
