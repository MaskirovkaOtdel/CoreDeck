using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Final_Efstathiadis_Theodors.Data;
using Final_Efstathiadis_Theodors.Models;
using Final_Efstathiadis_Theodors.Services;

namespace Final_Efstathiadis_Theodors.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartService _cartService;

        public WishlistController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            ICartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }

        // GET: Wishlist/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var items = await _context.WishlistItems
                .Include(w => w.Product)
                    .ThenInclude(p => p!.Category)
                .Where(w => w.UserId == user.Id)
                .OrderByDescending(w => w.AddedDate)
                .ToListAsync();

            return View(items);
        }

        // POST: Wishlist/ToggleWishlist
        [HttpPost]
        public async Task<IActionResult> ToggleWishlist(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Authentication required." });
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = "Product not found." });
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction("Index", "Product");
            }

            var existingItem = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.ProductId == productId);

            bool isAdded;
            if (existingItem != null)
            {
                _context.WishlistItems.Remove(existingItem);
                isAdded = false;
            }
            else
            {
                var newItem = new WishlistItem
                {
                    UserId = user.Id,
                    ProductId = productId,
                    AddedDate = DateTime.UtcNow
                };
                _context.WishlistItems.Add(newItem);
                isAdded = true;
            }

            await _context.SaveChangesAsync();

            var totalCount = await _context.WishlistItems.CountAsync(w => w.UserId == user.Id);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    isWishlisted = isAdded,
                    count = totalCount,
                    message = isAdded ? "Hardware saved to wishlist." : "Hardware removed from wishlist."
                });
            }

            TempData["SuccessMessage"] = isAdded
                ? $"'{product.Name}' added to your wishlist."
                : $"'{product.Name}' removed from your wishlist.";

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);

            return RedirectToAction(nameof(Index));
        }

        // POST: Wishlist/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var item = await _context.WishlistItems
                .Include(w => w.Product)
                .FirstOrDefaultAsync(w => w.Id == id && w.UserId == user.Id);

            if (item != null)
            {
                var productName = item.Product?.Name ?? "Item";
                _context.WishlistItems.Remove(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{productName}' removed from your wishlist.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Wishlist/MoveToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToCart(int productId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive || product.Stock <= 0)
            {
                TempData["ErrorMessage"] = "Product is currently out of stock or unavailable for requisition.";
                return RedirectToAction(nameof(Index));
            }

            // Add to cart
            var added = await _cartService.AddToCartAsync(user.Id, productId, 1);
            if (!added)
            {
                TempData["ErrorMessage"] = "Could not move item to cart.";
                return RedirectToAction(nameof(Index));
            }

            // Remove from wishlist
            var wishItem = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == user.Id && w.ProductId == productId);
            if (wishItem != null)
            {
                _context.WishlistItems.Remove(wishItem);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = $"'{product.Name}' transferred to requisition cart.";
            return RedirectToAction(nameof(Index));
        }
    }
}
