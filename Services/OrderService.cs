using Microsoft.EntityFrameworkCore;
using Final_Efstathiadis_Theodors.Data;
using Final_Efstathiadis_Theodors.Models;

namespace Final_Efstathiadis_Theodors.Services
{
    public interface IOrderService
    {
        Task<Order?> CreateOrderAsync(string userId, OrderCreationDto orderDto);
        Task<Order?> GetOrderAsync(int orderId, string userId);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task<List<Order>> GetAllOrdersAsync(OrderStatus? status = null, string? search = null);
        Task<bool> CancelOrderAsync(int orderId, string userId);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<decimal> CalculateOrderTotalAsync(List<OrderItemDto> items);
    }

    public class OrderDto
    {
        public string ShippingAddress { get; set; } = "";
        public string City { get; set; } = "";
        public string PostalCode { get; set; } = "";
        public string Country { get; set; } = "";
    }

    public class OrderCreationDto : OrderDto
    {
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Order?> CreateOrderAsync(string userId, OrderCreationDto orderDto)
        {
            if (string.IsNullOrEmpty(userId) || orderDto.Items.Count == 0)
                return null;

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                ShippingAddress = orderDto.ShippingAddress,
                City = orderDto.City,
                PostalCode = orderDto.PostalCode,
                Country = orderDto.Country
            };

            decimal totalPrice = 0;

            foreach (var itemDto in orderDto.Items)
            {
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null || !product.IsActive || product.Stock < itemDto.Quantity || itemDto.Quantity <= 0)
                    return null;

                // Enforce server-side authoritative database price to prevent client tampering
                var unitPrice = product.Price;
                var itemTotal = unitPrice * itemDto.Quantity;

                var orderItem = new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = unitPrice,
                    TotalPrice = itemTotal,
                    ProductSnapshot = product.Name
                };

                order.OrderItems.Add(orderItem);
                totalPrice += itemTotal;

                // Decrement stock
                product.Stock -= itemDto.Quantity;
            }

            decimal shipping = totalPrice >= 100 ? 0 : 9.99m;
            order.TotalPrice = totalPrice + shipping;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return order;
        }

        public async Task<Order?> GetOrderAsync(int orderId, string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllOrdersAsync(OrderStatus? status = null, string? search = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                if (int.TryParse(term, out int orderId))
                {
                    query = query.Where(o => o.Id == orderId || (o.User != null && (o.User.Email!.Contains(term) || o.User.FirstName.Contains(term) || o.User.LastName.Contains(term))));
                }
                else
                {
                    query = query.Where(o => o.User != null && (o.User.Email!.Contains(term) || o.User.FirstName.Contains(term) || o.User.LastName.Contains(term)));
                }
            }

            return await query.OrderByDescending(o => o.OrderDate).ToListAsync();
        }

        public async Task<bool> CancelOrderAsync(int orderId, string userId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
                return false;

            if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Processing)
            {
                order.Status = OrderStatus.Cancelled;

                // Restore stock
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.Stock += item.Quantity;
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return false;

            // If cancelling from non-cancelled, restore stock
            if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                foreach (var item in order.OrderItems)
                {
                    if (item.Product != null)
                    {
                        item.Product.Stock += item.Quantity;
                    }
                }
            }

            order.Status = status;

            if (status == OrderStatus.Shipped && !order.ShippedDate.HasValue)
                order.ShippedDate = DateTime.UtcNow;
            else if (status == OrderStatus.Delivered && !order.DeliveredDate.HasValue)
                order.DeliveredDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> CalculateOrderTotalAsync(List<OrderItemDto> items)
        {
            decimal total = 0;

            foreach (var item in items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null && product.IsActive)
                {
                    total += product.Price * item.Quantity;
                }
            }

            return total;
        }
    }
}
