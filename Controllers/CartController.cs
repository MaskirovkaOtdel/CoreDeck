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
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartService _cartService;

        public CartController(
            ApplicationDbContext context, 
            UserManager<ApplicationUser> userManager,
            ICartService cartService)
        {
            _context = context;
            _userManager = userManager;
            _cartService = cartService;
        }

        // GET: Cart/Index
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart { UserId = user.Id, CreatedDate = DateTime.UtcNow };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
                cart.CartItems = new List<CartItem>();
            }

            return View(cart);
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            if (quantity <= 0) quantity = 1;

            var success = await _cartService.AddToCartAsync(user.Id, productId, quantity);
            if (!success)
            {
                TempData["ErrorMessage"] = "Could not add item to cart. The product may be out of stock or inactive.";
            }
            else
            {
                TempData["SuccessMessage"] = "Item added to your requisition cart.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/RemoveFromCart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            await _cartService.RemoveFromCartAsync(user.Id, cartItemId);
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            await _cartService.UpdateQuantityAsync(user.Id, cartItemId, quantity);
            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || cart.CartItems.Count == 0)
                return RedirectToAction(nameof(Index));

            ViewData["User"] = user;
            return View(cart);
        }

        // POST: Cart/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(Order order)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || cart.CartItems.Count == 0)
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction(nameof(Index));
            }

            // Verify availability and prices against the database (authoritative check)
            decimal calculatedTotal = 0;
            var orderItemsToCreate = new List<OrderItem>();

            foreach (var item in cart.CartItems)
            {
                var liveProduct = await _context.Products.FindAsync(item.ProductId);
                if (liveProduct == null || !liveProduct.IsActive)
                {
                    TempData["ErrorMessage"] = $"Product '{item.Product?.Name ?? "Item"}' is no longer available in the arsenal.";
                    return RedirectToAction(nameof(Index));
                }

                if (liveProduct.Stock < item.Quantity)
                {
                    TempData["ErrorMessage"] = $"Insufficient stock for '{liveProduct.Name}'. Requested {item.Quantity}, only {liveProduct.Stock} available.";
                    return RedirectToAction(nameof(Index));
                }

                var unitPrice = liveProduct.Price;
                var itemTotal = unitPrice * item.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = liveProduct.Id,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = itemTotal,
                    ProductSnapshot = liveProduct.Name
                };

                orderItemsToCreate.Add(orderItem);
                calculatedTotal += itemTotal;

                // Decrement stock
                liveProduct.Stock -= item.Quantity;
            }

            // Calculate shipping (free over €100, otherwise €9.99)
            decimal shipping = calculatedTotal >= 100 ? 0 : 9.99m;

            // Create and persist order
            var newOrder = new Order
            {
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                ShippingAddress = order.ShippingAddress?.Trim() ?? "",
                City = order.City?.Trim() ?? "",
                PostalCode = order.PostalCode?.Trim() ?? "",
                Country = order.Country?.Trim() ?? "",
                TotalPrice = calculatedTotal + shipping,
                OrderItems = orderItemsToCreate
            };

            _context.Orders.Add(newOrder);

            // Clear cart
            _context.CartItems.RemoveRange(cart.CartItems);
            cart.LastModifiedDate = DateTime.UtcNow;

            // Update user profile coordinates if previously unconfigured
            if (string.IsNullOrWhiteSpace(user.Address) && !string.IsNullOrWhiteSpace(newOrder.ShippingAddress))
            {
                user.Address = newOrder.ShippingAddress;
                user.City = newOrder.City;
                user.PostalCode = newOrder.PostalCode;
                user.Country = newOrder.Country;
                await _userManager.UpdateAsync(user);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Order #{newOrder.Id} successfully placed and queued for dispatch!";
            return RedirectToAction("Details", "Order", new { id = newOrder.Id });
        }
    }
}
