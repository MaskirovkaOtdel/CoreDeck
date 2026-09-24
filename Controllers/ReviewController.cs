using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Final_Efstathiadis_Theodors.Data;
using Final_Efstathiadis_Theodors.Models;

namespace Final_Efstathiadis_Theodors.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReviewController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Review/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, int rating, string title, string comment)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Target product not found.";
                return RedirectToAction("Index", "Product");
            }

            if (rating < 1 || rating > 5)
            {
                TempData["ErrorMessage"] = "Rating must be between 1 and 5 stars.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(comment))
            {
                TempData["ErrorMessage"] = "Both a headline and review comments are required.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            // Check if user is a verified buyer (has any non-cancelled order containing this product)
            bool isVerifiedBuyer = await _context.Orders
                .Where(o => o.UserId == user.Id && o.Status != OrderStatus.Cancelled)
                .AnyAsync(o => o.OrderItems.Any(oi => oi.ProductId == productId));

            var review = new ProductReview
            {
                ProductId = productId,
                UserId = user.Id,
                Rating = rating,
                Title = title.Trim(),
                Comment = comment.Trim(),
                IsVerifiedBuyer = isVerifiedBuyer,
                CreatedDate = DateTime.UtcNow
            };

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your telemetry review has been broadcast successfully!";
            return RedirectToAction("Details", "Product", new { id = productId });
        }
    }
}
