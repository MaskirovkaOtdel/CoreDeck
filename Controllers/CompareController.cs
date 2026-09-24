using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Final_Efstathiadis_Theodors.Data;
using Final_Efstathiadis_Theodors.Models;

namespace Final_Efstathiadis_Theodors.Controllers
{
    public class CompareController : Controller
    {
        public const string SessionCompareKey = "CoreDeck_CompareList";
        private readonly ApplicationDbContext _context;

        public CompareController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper to retrieve compare product IDs from session
        public List<int> GetSessionCompareIds()
        {
            var raw = HttpContext.Session.GetString(SessionCompareKey);
            if (string.IsNullOrWhiteSpace(raw))
                return new List<int>();

            return raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                      .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                      .Where(id => id > 0)
                      .Distinct()
                      .Take(CompareViewModel.MaxProducts)
                      .ToList();
        }

        // Helper to persist compare product IDs to session
        public void SaveSessionCompareIds(List<int> ids)
        {
            var distinctIds = ids.Where(i => i > 0).Distinct().Take(CompareViewModel.MaxProducts).ToList();
            if (distinctIds.Any())
            {
                HttpContext.Session.SetString(SessionCompareKey, string.Join(",", distinctIds));
            }
            else
            {
                HttpContext.Session.Remove(SessionCompareKey);
            }
        }

        // GET: /Compare or /Compare?ids=1,2,3
        [HttpGet]
        public async Task<IActionResult> Index(string? ids)
        {
            List<int> idList;

            if (!string.IsNullOrWhiteSpace(ids))
            {
                idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                            .Where(id => id > 0)
                            .Distinct()
                            .Take(CompareViewModel.MaxProducts)
                            .ToList();
                SaveSessionCompareIds(idList);
            }
            else
            {
                idList = GetSessionCompareIds();
            }

            var products = new List<Product>();
            if (idList.Any())
            {
                var loaded = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Reviews)
                    .Where(p => idList.Contains(p.Id))
                    .ToListAsync();

                // Preserve the requested order
                foreach (var id in idList)
                {
                    var prod = loaded.FirstOrDefault(p => p.Id == id);
                    if (prod != null)
                    {
                        products.Add(prod);
                    }
                }
            }

            var model = new CompareViewModel
            {
                Products = products
            };

            return View(model);
        }

        // POST: /Compare/Add
        [HttpPost]
        public async Task<IActionResult> Add(int id)
        {
            if (id <= 0)
            {
                return IsAjaxRequest() 
                    ? Json(new { success = false, message = "Invalid product identifier." }) 
                    : RedirectToAction(nameof(Index));
            }

            var productExists = await _context.Products.AnyAsync(p => p.Id == id && p.IsActive);
            if (!productExists)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Product not found or inactive." })
                    : RedirectToAction(nameof(Index));
            }

            var currentIds = GetSessionCompareIds();

            if (currentIds.Contains(id))
            {
                return IsAjaxRequest()
                    ? Json(new { success = true, count = currentIds.Count, message = "Hardware already in comparison matrix." })
                    : RedirectBackOrIndex();
            }

            if (currentIds.Count >= CompareViewModel.MaxProducts)
            {
                var fullMsg = $"Comparison limit reached. Maximum {CompareViewModel.MaxProducts} products can be compared at once.";
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, count = currentIds.Count, message = fullMsg });
                }
                TempData["ErrorMessage"] = fullMsg;
                return RedirectBackOrIndex();
            }

            currentIds.Add(id);
            SaveSessionCompareIds(currentIds);

            if (IsAjaxRequest())
            {
                return Json(new { success = true, count = currentIds.Count, message = "Added to comparison matrix." });
            }

            TempData["SuccessMessage"] = "Hardware queued into comparison matrix.";
            return RedirectBackOrIndex();
        }

        // POST: /Compare/Remove
        [HttpPost]
        public IActionResult Remove(int id)
        {
            var currentIds = GetSessionCompareIds();
            if (currentIds.Remove(id))
            {
                SaveSessionCompareIds(currentIds);
            }

            if (IsAjaxRequest())
            {
                return Json(new { success = true, count = currentIds.Count, message = "Removed from comparison matrix." });
            }

            TempData["SuccessMessage"] = "Hardware removed from comparison matrix.";
            return RedirectBackOrIndex();
        }

        // POST: /Compare/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(SessionCompareKey);

            if (IsAjaxRequest())
            {
                return Json(new { success = true, count = 0, message = "Comparison matrix cleared." });
            }

            TempData["SuccessMessage"] = "Comparison matrix reset.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Compare/Count
        [HttpGet]
        public IActionResult Count()
        {
            var count = GetSessionCompareIds().Count;
            return Json(new { count });
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   Request.Headers.Accept.ToString().Contains("application/json");
        }

        private IActionResult RedirectBackOrIndex()
        {
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrWhiteSpace(referer) && Uri.IsWellFormedUriString(referer, UriKind.RelativeOrAbsolute))
            {
                return Redirect(referer);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
