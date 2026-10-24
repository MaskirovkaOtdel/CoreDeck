using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Final_Efstathiadis_Theodors.Controllers;
using Final_Efstathiadis_Theodors.Data;
using Final_Efstathiadis_Theodors.Models;
using Final_Efstathiadis_Theodors.Services;

namespace CoreDeck.Tests;

[TestClass]
public sealed class CartServiceTests
{
    private static ApplicationDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [TestMethod]
    public async Task AddToCart_ValidActiveProduct_CreatesCartAndAddsItem()
    {
        using var context = CreateInMemoryDbContext("Cart_AddValid");
        context.Categories.Add(new Category { Id = 1, Name = "GPUs" });
        context.Products.Add(new Product { Id = 1, Name = "RTX 5080", Price = 1199m, Stock = 5, IsActive = true, CategoryId = 1 });
        await context.SaveChangesAsync();

        var service = new CartService(context);
        var added = await service.AddToCartAsync("user-1", 1, 2);

        Assert.IsTrue(added);
        Assert.AreEqual(2, await service.GetCartItemCountAsync("user-1"));
        Assert.AreEqual(2398m, await service.GetCartTotalAsync("user-1"));
    }

    [TestMethod]
    public async Task AddToCart_QuantityExceedsStock_ClampsToAvailableStock()
    {
        using var context = CreateInMemoryDbContext("Cart_ClampStock");
        context.Categories.Add(new Category { Id = 1, Name = "CPUs" });
        context.Products.Add(new Product { Id = 2, Name = "Ryzen 7 9800X3D", Price = 499m, Stock = 3, IsActive = true, CategoryId = 1 });
        await context.SaveChangesAsync();

        var service = new CartService(context);
        var added = await service.AddToCartAsync("user-2", 2, 10);

        Assert.IsTrue(added);
        Assert.AreEqual(3, await service.GetCartItemCountAsync("user-2"));
        Assert.AreEqual(1497m, await service.GetCartTotalAsync("user-2"));
    }

    [TestMethod]
    public async Task AddToCart_OutOfStockOrInactiveOrInvalidQuantity_ReturnsFalse()
    {
        using var context = CreateInMemoryDbContext("Cart_InvalidMutation");
        context.Categories.Add(new Category { Id = 1, Name = "GPUs" });
        context.Products.AddRange(
            new Product { Id = 10, Name = "Zero Stock GPU", Price = 800m, Stock = 0, IsActive = true, CategoryId = 1 },
            new Product { Id = 11, Name = "Inactive GPU", Price = 800m, Stock = 5, IsActive = false, CategoryId = 1 },
            new Product { Id = 12, Name = "Active GPU", Price = 800m, Stock = 5, IsActive = true, CategoryId = 1 }
        );
        await context.SaveChangesAsync();

        var service = new CartService(context);

        Assert.IsFalse(await service.AddToCartAsync("u1", 10, 1));
        Assert.IsFalse(await service.AddToCartAsync("u1", 11, 1));
        Assert.IsFalse(await service.AddToCartAsync("u1", 12, 0));
        Assert.IsFalse(await service.AddToCartAsync("u1", 12, -3));
        Assert.IsFalse(await service.AddToCartAsync("u1", 999, 1));
    }

    [TestMethod]
    public async Task UpdateQuantity_ZeroOrNegative_RemovesCartItem()
    {
        using var context = CreateInMemoryDbContext("Cart_UpdateZeroRemoves");
        context.Categories.Add(new Category { Id = 1, Name = "RAM" });
        context.Products.Add(new Product { Id = 3, Name = "32GB DDR5", Price = 129m, Stock = 10, IsActive = true, CategoryId = 1 });
        await context.SaveChangesAsync();

        var service = new CartService(context);
        await service.AddToCartAsync("user-3", 3, 2);
        var cart = await service.GetOrCreateCartAsync("user-3");
        var itemId = cart.CartItems.First().Id;

        var updated = await service.UpdateQuantityAsync("user-3", itemId, 0);
        Assert.IsTrue(updated);
        Assert.AreEqual(0, await service.GetCartItemCountAsync("user-3"));
    }

    [TestMethod]
    public async Task RemoveFromCart_WrongUser_RejectsCrossUserTampering()
    {
        using var context = CreateInMemoryDbContext("Cart_CrossUserGuard");
        context.Categories.Add(new Category { Id = 1, Name = "Monitors" });
        context.Products.Add(new Product { Id = 4, Name = "OLED 240Hz", Price = 799m, Stock = 4, IsActive = true, CategoryId = 1 });
        await context.SaveChangesAsync();

        var service = new CartService(context);
        await service.AddToCartAsync("owner-user", 4, 1);
        var cart = await service.GetOrCreateCartAsync("owner-user");
        var itemId = cart.CartItems.First().Id;

        var removedByAttacker = await service.RemoveFromCartAsync("attacker-user", itemId);
        Assert.IsFalse(removedByAttacker);
        Assert.AreEqual(1, await service.GetCartItemCountAsync("owner-user"));
    }

    [TestMethod]
    public async Task ClearCart_EmptiesAllItemsForUser()
    {
        using var context = CreateInMemoryDbContext("Cart_Clear");
        context.Categories.Add(new Category { Id = 1, Name = "Peripherals" });
        context.Products.AddRange(
            new Product { Id = 5, Name = "Mouse", Price = 79m, Stock = 10, IsActive = true, CategoryId = 1 },
            new Product { Id = 6, Name = "Keyboard", Price = 149m, Stock = 10, IsActive = true, CategoryId = 1 }
        );
        await context.SaveChangesAsync();

        var service = new CartService(context);
        await service.AddToCartAsync("user-clear", 5, 1);
        await service.AddToCartAsync("user-clear", 6, 2);
        Assert.AreEqual(3, await service.GetCartItemCountAsync("user-clear"));

        await service.ClearCartAsync("user-clear");
        Assert.AreEqual(0, await service.GetCartItemCountAsync("user-clear"));
        Assert.AreEqual(0m, await service.GetCartTotalAsync("user-clear"));
    }
}

[TestClass]
public sealed class OrderServiceTests
{
    private static ApplicationDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [TestMethod]
    public async Task CreateOrder_EnforcesDatabasePriceAndDeductsStock()
    {
        using var context = CreateInMemoryDbContext("Order_AuthoritativePrice");
        context.Users.Add(new ApplicationUser { Id = "buyer-1", UserName = "buyer1@test.com", Email = "buyer1@test.com" });
        context.Categories.Add(new Category { Id = 1, Name = "GPUs" });
        context.Products.Add(new Product { Id = 1, Name = "RTX 5090", Price = 1999m, Stock = 5, IsActive = true, CategoryId = 1 });
        await context.SaveChangesAsync();

        var service = new OrderService(context);
        var dto = new OrderCreationDto
        {
            ShippingAddress = "Cyber Way 42",
            City = "Athens",
            PostalCode = "10431",
            Country = "Greece",
            Items = new List<OrderItemDto>
            {
                // Intentionally pass tampered 1.00m client UnitPrice; service must enforce DB price 1999m
                new OrderItemDto { ProductId = 1, Quantity = 2, UnitPrice = 1.00m }
            }
        };

        var order = await service.CreateOrderAsync("buyer-1", dto);

        Assert.IsNotNull(order);
        Assert.AreEqual(3998m, order.TotalPrice);
        var updatedProduct = await context.Products.FindAsync(1);
        Assert.AreEqual(3, updatedProduct!.Stock);
    }

    [TestMethod]
    public async Task CreateOrder_SubtotalUnder100_AddsFlatShippingFee()
    {
        using var context = CreateInMemoryDbContext("Order_ShippingFee");
        context.Users.Add(new ApplicationUser { Id = "buyer-2", UserName = "buyer2@test.com", Email = "buyer2@test.com" });
        context.Categories.Add(new Category { Id = 1, Name = "Accessories" });
        context.Products.Add(new Product { Id = 2, Name = "Thermal Paste", Price = 20m, Stock = 10, IsActive = true, CategoryId = 1 });
        await context.SaveChangesAsync();

        var service = new OrderService(context);
        var dto = new OrderCreationDto
        {
            ShippingAddress = "Main St 1",
            City = "Athens",
            PostalCode = "10000",
            Country = "Greece",
            Items = new List<OrderItemDto>
            {
                new OrderItemDto { ProductId = 2, Quantity = 2, UnitPrice = 20m }
            }
        };

        var order = await service.CreateOrderAsync("buyer-2", dto);
        Assert.IsNotNull(order);
        Assert.AreEqual(49.99m, order.TotalPrice);
    }

    [TestMethod]
    public async Task CreateOrder_InsufficientStockOrNegativeQuantity_RejectsOrder()
    {
        using var context = CreateInMemoryDbContext("Order_StockRejection");
        context.Users.Add(new ApplicationUser { Id = "buyer-3", UserName = "buyer3@test.com", Email = "buyer3@test.com" });
        context.Categories.Add(new Category { Id = 1, Name = "CPUs" });
        context.Products.Add(new Product { Id = 3, Name = "Core Ultra 9", Price = 589m, Stock = 1, IsActive = true, CategoryId = 1 });
        await context.SaveChangesAsync();

        var service = new OrderService(context);
        var overStockDto = new OrderCreationDto
        {
            ShippingAddress = "Addr",
            City = "City",
            PostalCode = "12345",
            Country = "GR",
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = 3, Quantity = 2 } }
        };

        var result = await service.CreateOrderAsync("buyer-3", overStockDto);
        Assert.IsNull(result);
        var product = await context.Products.FindAsync(3);
        Assert.AreEqual(1, product!.Stock);
    }

    [TestMethod]
    public async Task CancelOrder_WhenPending_RestoresStockAndMarksCancelled()
    {
        using var context = CreateInMemoryDbContext("Order_CancelRestoresStock");
        context.Users.Add(new ApplicationUser { Id = "buyer-4", UserName = "buyer4@test.com", Email = "buyer4@test.com" });
        context.Categories.Add(new Category { Id = 1, Name = "Handhelds" });
        context.Products.Add(new Product { Id = 4, Name = "Steam Deck OLED", Price = 679m, Stock = 5, IsActive = true, CategoryId = 1 });
        await context.SaveChangesAsync();

        var service = new OrderService(context);
        var order = await service.CreateOrderAsync("buyer-4", new OrderCreationDto
        {
            ShippingAddress = "Addr",
            City = "City",
            PostalCode = "11111",
            Country = "GR",
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = 4, Quantity = 2 } }
        });

        Assert.IsNotNull(order);
        Assert.AreEqual(3, (await context.Products.FindAsync(4))!.Stock);

        var cancelled = await service.CancelOrderAsync(order.Id, "buyer-4");
        Assert.IsTrue(cancelled);
        Assert.AreEqual(5, (await context.Products.FindAsync(4))!.Stock);
        Assert.AreEqual(OrderStatus.Cancelled, (await service.GetOrderByIdAsync(order.Id))!.Status);
    }

    [TestMethod]
    public async Task UpdateOrderStatus_TransitionsToShippedAndDelivered_SetsTimestamps()
    {
        using var context = CreateInMemoryDbContext("Order_StatusTimestamps");
        context.Users.Add(new ApplicationUser { Id = "buyer-5", UserName = "buyer5@test.com", Email = "buyer5@test.com" });
        context.Categories.Add(new Category { Id = 1, Name = "Laptops" });
        context.Products.Add(new Product { Id = 5, Name = "Blade 16", Price = 2999m, Stock = 3, IsActive = true, CategoryId = 1 });
        await context.SaveChangesAsync();

        var service = new OrderService(context);
        var order = await service.CreateOrderAsync("buyer-5", new OrderCreationDto
        {
            ShippingAddress = "Addr",
            City = "City",
            PostalCode = "11111",
            Country = "GR",
            Items = new List<OrderItemDto> { new OrderItemDto { ProductId = 5, Quantity = 1 } }
        });

        Assert.IsTrue(await service.UpdateOrderStatusAsync(order!.Id, OrderStatus.Shipped));
        var shippedOrder = await service.GetOrderByIdAsync(order.Id);
        Assert.IsNotNull(shippedOrder!.ShippedDate);

        Assert.IsTrue(await service.UpdateOrderStatusAsync(order.Id, OrderStatus.Delivered));
        var deliveredOrder = await service.GetOrderByIdAsync(order.Id);
        Assert.IsNotNull(deliveredOrder!.DeliveredDate);
    }
}

[TestClass]
public sealed class ProductCatalogFilterTests
{
    private static ApplicationDbContext CreateSeededContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        var context = new ApplicationDbContext(options);

        context.Categories.AddRange(
            new Category { Id = 1, Name = "GPUs" },
            new Category { Id = 2, Name = "CPUs" }
        );

        context.Products.AddRange(
            new Product { Id = 1, Name = "NVIDIA GeForce RTX 5090", Description = "Flagship 32GB GDDR7 GPU", Price = 1999m, Stock = 4, IsActive = true, CategoryId = 1, CreatedDate = DateTime.UtcNow.AddDays(-1) },
            new Product { Id = 2, Name = "AMD Radeon RX 8800 XT", Description = "High value RDNA4 GPU", Price = 599m, Stock = 0, IsActive = true, CategoryId = 1, CreatedDate = DateTime.UtcNow.AddDays(-2) },
            new Product { Id = 3, Name = "AMD Ryzen 9 9950X", Description = "16-Core AM5 Processor", Price = 649m, Stock = 12, IsActive = true, CategoryId = 2, CreatedDate = DateTime.UtcNow.AddDays(-3) },
            new Product { Id = 4, Name = "Intel Core Ultra 9 285K", Description = "Arrow Lake Desktop CPU", Price = 589m, Stock = 7, IsActive = true, CategoryId = 2, CreatedDate = DateTime.UtcNow.AddDays(-4) }
        );
        context.SaveChanges();
        return context;
    }

    private static ProductController CreateControllerWithHttpContext(ApplicationDbContext context)
    {
        return new ProductController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };
    }

    [TestMethod]
    public async Task Index_FilterByCategoryAndInStock_ReturnsMatchingActiveProducts()
    {
        using var context = CreateSeededContext("Catalog_CategoryInStock");
        var controller = CreateControllerWithHttpContext(context);

        var result = await controller.Index(categoryId: 1, inStockOnly: true) as ViewResult;
        Assert.IsNotNull(result);
        var model = result.Model as List<Product>;
        Assert.IsNotNull(model);
        Assert.HasCount(1, model);
        Assert.AreEqual("NVIDIA GeForce RTX 5090", model[0].Name);
    }

    [TestMethod]
    public async Task Index_FilterByPriceRangeAndSortAsc_ReturnsOrderedSubset()
    {
        using var context = CreateSeededContext("Catalog_PriceSort");
        var controller = CreateControllerWithHttpContext(context);

        var result = await controller.Index(minPrice: 550m, maxPrice: 700m, sortOrder: "price_asc") as ViewResult;
        Assert.IsNotNull(result);
        var model = result.Model as List<Product>;
        Assert.IsNotNull(model);
        Assert.HasCount(3, model);
        Assert.AreEqual(589m, model[0].Price);
        Assert.AreEqual(599m, model[1].Price);
        Assert.AreEqual(649m, model[2].Price);
    }

    [TestMethod]
    public async Task Index_FilterBySearchKeyword_MatchesNameAndDescription()
    {
        using var context = CreateSeededContext("Catalog_SearchKeyword");
        var controller = CreateControllerWithHttpContext(context);

        var result = await controller.Index(search: "AMD") as ViewResult;
        Assert.IsNotNull(result);
        var model = result.Model as List<Product>;
        Assert.IsNotNull(model);
        Assert.HasCount(2, model);
    }

    [TestMethod]
    public async Task Index_FilterByBrandCaseInsensitive_ReturnsBrandMatches()
    {
        using var context = CreateSeededContext("Catalog_BrandFilter");
        var controller = new ProductController(context); // Also verifies null-safe User principal without HttpContext

        var result = await controller.Index(brand: "  intel ") as ViewResult;
        Assert.IsNotNull(result);
        var model = result.Model as List<Product>;
        Assert.IsNotNull(model);
        Assert.HasCount(1, model);
        Assert.AreEqual("Intel Core Ultra 9 285K", model[0].Name);
        Assert.AreEqual("intel", result.ViewData["SelectedBrand"]);
    }

    [TestMethod]
    public async Task Details_ValidProductWithoutHttpContext_ReturnsViewSafely()
    {
        using var context = CreateSeededContext("Catalog_DetailsSafe");
        var controller = new ProductController(context);

        var result = await controller.Details(1) as ViewResult;
        Assert.IsNotNull(result);
        var model = result.Model as Product;
        Assert.IsNotNull(model);
        Assert.AreEqual("NVIDIA GeForce RTX 5090", model.Name);
    }
}

[TestClass]
public sealed class ReviewsAndRatingsTests
{
    private static ApplicationDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [TestMethod]
    public async Task Reviews_VerifiedBuyerCheck_WhenUserHasNonCancelledOrder_ShouldBeTrue()
    {
        using var context = CreateInMemoryDbContext("Comm_VerifiedBuyer_True");
        var user = new ApplicationUser { Id = "user1", UserName = "buyer@test.com", Email = "buyer@test.com" };
        var cat = new Category { Id = 1, Name = "GPUs" };
        var product = new Product { Id = 10, Name = "RTX 5090", Price = 1999m, CategoryId = 1, Stock = 5, IsActive = true };
        context.Users.Add(user);
        context.Categories.Add(cat);
        context.Products.Add(product);

        context.Orders.Add(new Order
        {
            Id = 101,
            UserId = user.Id,
            Status = OrderStatus.Processing,
            OrderDate = DateTime.UtcNow,
            OrderItems = new List<OrderItem>
            {
                new OrderItem { Id = 1, ProductId = product.Id, Quantity = 1, UnitPrice = 1999m, TotalPrice = 1999m }
            }
        });
        await context.SaveChangesAsync();

        bool isVerified = await context.Orders
            .Where(o => o.UserId == user.Id && o.Status != OrderStatus.Cancelled)
            .AnyAsync(o => o.OrderItems.Any(oi => oi.ProductId == product.Id));

        Assert.IsTrue(isVerified);
    }

    [TestMethod]
    public async Task Reviews_VerifiedBuyerCheck_WhenUserOnlyHasCancelledOrder_ShouldBeFalse()
    {
        using var context = CreateInMemoryDbContext("Comm_VerifiedBuyer_Cancelled");
        var user = new ApplicationUser { Id = "user2", UserName = "cancel@test.com", Email = "cancel@test.com" };
        var cat = new Category { Id = 1, Name = "GPUs" };
        var product = new Product { Id = 11, Name = "RTX 5080", Price = 1199m, CategoryId = 1, Stock = 5, IsActive = true };
        context.Users.Add(user);
        context.Categories.Add(cat);
        context.Products.Add(product);

        context.Orders.Add(new Order
        {
            Id = 102,
            UserId = user.Id,
            Status = OrderStatus.Cancelled,
            OrderDate = DateTime.UtcNow,
            OrderItems = new List<OrderItem>
            {
                new OrderItem { Id = 2, ProductId = product.Id, Quantity = 1, UnitPrice = 1199m, TotalPrice = 1199m }
            }
        });
        await context.SaveChangesAsync();

        bool isVerified = await context.Orders
            .Where(o => o.UserId == user.Id && o.Status != OrderStatus.Cancelled)
            .AnyAsync(o => o.OrderItems.Any(oi => oi.ProductId == product.Id));

        Assert.IsFalse(isVerified);
    }

    [TestMethod]
    public async Task Reviews_AverageRatingComputation_WithMultipleRatings_ComputesAccurateAverage()
    {
        using var context = CreateInMemoryDbContext("Comm_Reviews_AvgRating");
        context.Categories.Add(new Category { Id = 1, Name = "CPUs" });
        context.Products.Add(new Product { Id = 20, Name = "Ryzen 9 9950X", Price = 649m, CategoryId = 1, Stock = 10, IsActive = true });

        context.ProductReviews.AddRange(
            new ProductReview { Id = 1, ProductId = 20, UserId = "u1", Rating = 5, Title = "Great", Comment = "Fast", IsVerifiedBuyer = true },
            new ProductReview { Id = 2, ProductId = 20, UserId = "u2", Rating = 4, Title = "Good", Comment = "Solid", IsVerifiedBuyer = false },
            new ProductReview { Id = 3, ProductId = 20, UserId = "u3", Rating = 4, Title = "Solid", Comment = "Reliable", IsVerifiedBuyer = true },
            new ProductReview { Id = 4, ProductId = 20, UserId = "u4", Rating = 3, Title = "Okay", Comment = "Hot", IsVerifiedBuyer = false }
        );
        await context.SaveChangesAsync();

        var reviews = await context.ProductReviews.Where(r => r.ProductId == 20).ToListAsync();
        Assert.HasCount(4, reviews);
        Assert.AreEqual(4.0, reviews.Average(r => r.Rating), 0.001);
        Assert.AreEqual(2, reviews.Count(r => r.IsVerifiedBuyer));
    }
}

[TestClass]
public sealed class WishlistTests
{
    private static ApplicationDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ApplicationDbContext(options);
    }

    [TestMethod]
    public async Task Wishlist_ToggleWishlist_AddsWhenNotPresent_RemovesWhenPresent()
    {
        using var context = CreateInMemoryDbContext("Comm_Wishlist_Toggle");
        var user = new ApplicationUser { Id = "wishUser", UserName = "wish@test.com", Email = "wish@test.com" };
        var cat = new Category { Id = 1, Name = "Monitors" };
        var product = new Product { Id = 30, Name = "OLED 4K Monitor", Price = 899m, CategoryId = 1, Stock = 5, IsActive = true };
        context.Users.Add(user);
        context.Categories.Add(cat);
        context.Products.Add(product);
        await context.SaveChangesAsync();

        context.WishlistItems.Add(new WishlistItem { UserId = user.Id, ProductId = product.Id, AddedDate = DateTime.UtcNow });
        await context.SaveChangesAsync();
        Assert.AreEqual(1, await context.WishlistItems.CountAsync(w => w.UserId == user.Id));

        var itemToRemove = await context.WishlistItems.FirstAsync(w => w.UserId == user.Id && w.ProductId == product.Id);
        context.WishlistItems.Remove(itemToRemove);
        await context.SaveChangesAsync();
        Assert.AreEqual(0, await context.WishlistItems.CountAsync(w => w.UserId == user.Id));
    }

    [TestMethod]
    public async Task Wishlist_MoveToCart_WhenOutOfStock_FailsGracefully()
    {
        using var context = CreateInMemoryDbContext("Comm_Wishlist_OutOfStock");
        var user = new ApplicationUser { Id = "oosUser", UserName = "oos@test.com", Email = "oos@test.com" };
        context.Categories.Add(new Category { Id = 1, Name = "GPUs" });
        context.Products.Add(new Product { Id = 32, Name = "Rare GPU", Price = 999m, CategoryId = 1, Stock = 0, IsActive = true });
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var cartService = new CartService(context);
        Assert.IsFalse(await cartService.AddToCartAsync(user.Id, 32, 1));
    }
}

[TestClass]
public sealed class CompareMatrixBoundsTests
{
    [TestMethod]
    public void CompareViewModel_Empty_ReturnsSafeDefaults()
    {
        var vm = new CompareViewModel();
        Assert.IsFalse(vm.HasProducts);
        Assert.IsTrue(vm.CanAddMore);
        Assert.AreEqual(0, vm.SlotCount);
        Assert.IsNull(vm.GetLowestPrice());
        Assert.IsNull(vm.GetHighestPrice());
    }

    [TestMethod]
    public void CompareViewModel_Bounds_CannotAddMoreThanMaxProducts()
    {
        var vm = new CompareViewModel();
        for (int i = 1; i <= CompareViewModel.MaxProducts; i++)
        {
            vm.Products.Add(new Product { Id = i, Name = $"Product {i}", Price = 100m * i });
        }

        Assert.IsTrue(vm.HasProducts);
        Assert.IsFalse(vm.CanAddMore);
        Assert.AreEqual(CompareViewModel.MaxProducts, vm.SlotCount);
    }

    [TestMethod]
    public void CompareViewModel_PriceBounds_CalculatesMinAndMaxCorrectly()
    {
        var vm = new CompareViewModel();
        vm.Products.Add(new Product { Id = 1, Name = "Budget GPU", Price = 250m });
        vm.Products.Add(new Product { Id = 2, Name = "Mid GPU", Price = 650m });
        vm.Products.Add(new Product { Id = 3, Name = "Apex GPU", Price = 1899m });

        Assert.AreEqual(250m, vm.GetLowestPrice());
        Assert.AreEqual(1899m, vm.GetHighestPrice());
    }
}

[TestClass]
public sealed class SmartReleaseSemVerLogicTests
{
    /// <summary>
    /// Mirrors the exact commit classification and SemVer calculation rules used in .github/workflows/smart-release.yml
    /// </summary>
    private static (bool ShouldRelease, string NextTag, string BumpType) EvaluateSemVerRelease(string currentTag, IEnumerable<string> commitSubjects)
    {
        string bump = "none";
        foreach (var subject in commitSubjects)
        {
            if (string.IsNullOrWhiteSpace(subject)) continue;
            if (Regex.IsMatch(subject, @"(\[skip release\]|\[no release\]|^Merge )")) continue;

            if (Regex.IsMatch(subject, @"(BREAKING CHANGE|^[a-zA-Z]+(\([^)]+\))?!:|^major(\([^)]+\))?:)"))
            {
                bump = "major";
                break;
            }

            if (Regex.IsMatch(subject, @"^(feat|update)(\([^)]+\))?:"))
            {
                bump = "minor";
            }

            if (bump != "minor" && Regex.IsMatch(subject, @"^(fix|hotfix|perf|security)(\([^)]+\))?:"))
            {
                bump = "patch";
            }
        }

        if (bump == "none")
        {
            return (false, currentTag, "none");
        }

        var parts = currentTag.TrimStart('v').Split('.').Select(int.Parse).ToArray();
        int major = parts[0], minor = parts[1], patch = parts[2];

        switch (bump)
        {
            case "major":
                major++;
                minor = 0;
                patch = 0;
                break;
            case "minor":
                minor++;
                patch = 0;
                break;
            case "patch":
                patch++;
                break;
        }

        return (true, $"v{major}.{minor}.{patch}", bump);
    }

    [TestMethod]
    public void SemVer_SmallFixCommits_BumpsPatchOnly_X_X_1_To_X_X_2()
    {
        var result = EvaluateSemVerRelease("v0.4.1", new[]
        {
            "docs: update readme typo",
            "fix(cart): prevent negative quantity on rapid click"
        });

        Assert.IsTrue(result.ShouldRelease);
        Assert.AreEqual("patch", result.BumpType);
        Assert.AreEqual("v0.4.2", result.NextTag);
    }

    [TestMethod]
    public void SemVer_FeatureUpdateCommits_BumpsMinorAndResetsPatch_X_4_X_To_X_5_0()
    {
        var result = EvaluateSemVerRelease("v0.4.2", new[]
        {
            "fix(ui): align navbar badge",
            "feat(catalog): add brand filtering and health probe",
            "test(catalog): add brand filter unit tests"
        });

        Assert.IsTrue(result.ShouldRelease);
        Assert.AreEqual("minor", result.BumpType);
        Assert.AreEqual("v0.5.0", result.NextTag);
    }

    [TestMethod]
    public void SemVer_BreakingChangeCommits_BumpsMajorChangebreaker_1_X_X_To_2_0_0()
    {
        var result = EvaluateSemVerRelease("v1.5.3", new[]
        {
            "feat(api)!: migrate storefront schema to v2 contract"
        });

        Assert.IsTrue(result.ShouldRelease);
        Assert.AreEqual("major", result.BumpType);
        Assert.AreEqual("v2.0.0", result.NextTag);
    }

    [TestMethod]
    public void SemVer_OnlyDocsChoreTestCiCommits_SkipsReleaseToPreventTagSpam()
    {
        var result = EvaluateSemVerRelease("v0.4.0", new[]
        {
            "docs(readme): clarify local setup steps",
            "chore(git): configure pre-commit leak guard",
            "ci(actions): update workflow runner matrix",
            "test(community): add unit tests for cart"
        });

        Assert.IsFalse(result.ShouldRelease);
        Assert.AreEqual("none", result.BumpType);
        Assert.AreEqual("v0.4.0", result.NextTag);
    }
}
