using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Final_Efstathiadis_Theodors.Data;
using Final_Efstathiadis_Theodors.Models;

namespace Final_Efstathiadis_Theodors.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser>? _userManager;
        private const int PageSize = 12;

        public ProductController(ApplicationDbContext context, UserManager<ApplicationUser>? userManager = null)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Product/Index
        public async Task<IActionResult> Index(
            int categoryId = 0,
            int page = 1,
            string search = "",
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool inStockOnly = false,
            string sortOrder = "newest")
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            // Filter by category
            if (categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // Filter by search keyword
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p => p.Name.Contains(term) || p.Description.Contains(term));
            }

            // Filter by price bounds
            if (minPrice.HasValue && minPrice.Value > 0)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Filter by stock availability
            if (inStockOnly)
            {
                query = query.Where(p => p.Stock > 0);
            }

            // Sorting logic
            query = sortOrder?.ToLowerInvariant() switch
            {
                "price_asc" or "priceasc" => query.OrderBy(p => p.Price),
                "price_desc" or "pricedesc" => query.OrderByDescending(p => p.Price),
                "name" => query.OrderBy(p => p.Name),
                _ => query.OrderByDescending(p => p.CreatedDate)
            };

            // Pagination
            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)PageSize));
            page = Math.Clamp(page, 1, totalPages);

            var products = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewData["CategoryId"] = categoryId;
            ViewData["SearchTerm"] = search;
            ViewData["MinPrice"] = minPrice;
            ViewData["MaxPrice"] = maxPrice;
            ViewData["InStockOnly"] = inStockOnly;
            ViewData["SortOrder"] = sortOrder ?? "newest";
            ViewData["CurrentPage"] = page;
            ViewData["TotalPages"] = totalPages;
            ViewData["TotalCount"] = totalCount;
            ViewData["Categories"] = await _context.Categories.ToListAsync();

            if (User.Identity?.IsAuthenticated == true && _userManager != null)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var wishlistIds = await _context.WishlistItems
                        .Where(w => w.UserId == user.Id)
                        .Select(w => w.ProductId)
                        .ToListAsync();
                    ViewBag.WishlistProductIds = new HashSet<int>(wishlistIds);
                }
            }

            return View(products);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
                return NotFound();

            ViewData["Categories"] = await _context.Categories.ToListAsync();

            if (User.Identity?.IsAuthenticated == true && _userManager != null)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewBag.IsWishlisted = await _context.WishlistItems
                        .AnyAsync(w => w.UserId == user.Id && w.ProductId == product.Id);
                    ViewBag.IsVerifiedBuyer = await _context.Orders
                        .Where(o => o.UserId == user.Id && o.Status != OrderStatus.Cancelled)
                        .AnyAsync(o => o.OrderItems.Any(oi => oi.ProductId == product.Id));
                }
            }

            return View(product);
        }

        // GET: Product/Search
        public IActionResult Search(string searchTerm)
        {
            return RedirectToAction(nameof(Index), new { search = searchTerm });
        }
    }
}
