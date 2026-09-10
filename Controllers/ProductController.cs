using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Final_Efstathiadis_Theodors.Data;
using Final_Efstathiadis_Theodors.Models;

namespace Final_Efstathiadis_Theodors.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const int PageSize = 12;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
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

            return View(products);
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
                return NotFound();

            ViewData["Categories"] = await _context.Categories.ToListAsync();
            return View(product);
        }

        // GET: Product/Search
        public IActionResult Search(string searchTerm)
        {
            return RedirectToAction(nameof(Index), new { search = searchTerm });
        }
    }
}
