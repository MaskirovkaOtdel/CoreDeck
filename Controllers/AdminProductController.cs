using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Final_Efstathiadis_Theodors.Data;
using Final_Efstathiadis_Theodors.Models;

namespace Final_Efstathiadis_Theodors.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Product")]
    public class AdminProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Product
        [HttpGet("")]
        public async Task<IActionResult> Index(bool showInactive = true)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!showInactive)
            {
                query = query.Where(p => p.IsActive);
            }

            var products = await query
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            ViewData["ShowInactive"] = showInactive;
            return View(products);
        }

        // GET: Admin/Product/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Categories"] = await _context.Categories.ToListAsync();
            return View(new Product { IsActive = true });
        }

        // POST: Admin/Product/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,Stock,ImageUrl,CategoryId,IsActive")] Product product)
        {
            if (ModelState.IsValid)
            {
                product.CreatedDate = DateTime.UtcNow;
                _context.Add(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Product '{product.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = await _context.Categories.ToListAsync();
            return View(product);
        }

        // GET: Admin/Product/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            ViewData["Categories"] = await _context.Categories.ToListAsync();
            return View(product);
        }

        // POST: Admin/Product/Edit/5
        [HttpPost("Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Stock,ImageUrl,CategoryId,CreatedDate,IsActive")] Product product)
        {
            if (id != product.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    product.UpdatedDate = DateTime.UtcNow;
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Product '{product.Name}' updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = await _context.Categories.ToListAsync();
            return View(product);
        }

        // GET: Admin/Product/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Admin/Product/Delete/5
        [HttpPost("Delete/{id}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Soft delete: marks as inactive to preserve foreign key integrity with existing orders
                product.IsActive = false;
                product.UpdatedDate = DateTime.UtcNow;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Product '{product.Name}' deactivated (soft-deleted).";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Product/Restore/5
        [HttpPost("Restore/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = true;
                product.UpdatedDate = DateTime.UtcNow;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Product '{product.Name}' restored to active catalog.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(p => p.Id == id);
        }
    }
}
