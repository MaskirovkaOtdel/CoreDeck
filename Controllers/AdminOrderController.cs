using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Final_Efstathiadis_Theodors.Data;
using Final_Efstathiadis_Theodors.Models;
using Final_Efstathiadis_Theodors.Services;

namespace Final_Efstathiadis_Theodors.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Orders")]
    public class AdminOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IOrderService _orderService;

        public AdminOrderController(ApplicationDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        // GET: Admin/Orders
        [HttpGet("")]
        public async Task<IActionResult> Index(OrderStatus? status = null, string? search = null)
        {
            var orders = await _orderService.GetAllOrdersAsync(status, search);

            ViewData["CurrentStatus"] = status;
            ViewData["SearchTerm"] = search;

            return View(orders);
        }

        // GET: Admin/Orders/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound();

            return View(order);
        }

        // POST: Admin/Orders/UpdateStatus/5
        [HttpPost("UpdateStatus/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status)
        {
            var success = await _orderService.UpdateOrderStatusAsync(id, status);
            if (success)
            {
                TempData["SuccessMessage"] = $"Order #{id} status updated to {status}.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to update order #{id}.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
