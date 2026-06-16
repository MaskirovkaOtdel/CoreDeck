using Final_Efstathiadis_Theodors.Data;
using Final_Efstathiadis_Theodors.Models;
using Microsoft.EntityFrameworkCore;

namespace Final_Efstathiadis_Theodors.Services
{
    public class DatabaseInitializerService
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            // Check if data already exists
            if (context.Categories.Any())
                return;

            // Add Categories
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "PCs & Desktops",
                    Description = "High-performance desktop computers and gaming PCs",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/1/1995.png"
                },
                new Category
                {
                    Name = "Laptops",
                    Description = "Portable computers for work and gaming",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/2/2742.png"
                },
                new Category
                {
                    Name = "Gaming Consoles",
                    Description = "Latest gaming consoles and accessories",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/1/1143.png"
                },
                new Category
                {
                    Name = "Handheld Devices",
                    Description = "Portable gaming and productivity devices",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/1/1182.png"
                },
                new Category
                {
                    Name = "Monitors & Peripherals",
                    Description = "Displays, keyboards, mice, and other peripherals",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/2/2841.png"
                },
                new Category
                {
                    Name = "Components & Upgrades",
                    Description = "CPUs, GPUs, RAM, SSDs, and other hardware components",
                    IconUrl = "https://cdn-icons-png.flaticon.com/512/1/1987.png"
                }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            // Get categories for foreign key references
            var pcCategory = categories[0];
            var laptopCategory = categories[1];
            var consoleCategory = categories[2];
            var handheldCategory = categories[3];
            var peripheralCategory = categories[4];
            var componentCategory = categories[5];

            // Add Products
            var products = new List<Product>
            {
                // PCs & Desktops
                new Product
                {
                    Name = "ASUS ROG STRIX Gaming PC",
                    Description = "High-end gaming desktop with RTX 4090, Intel i9-13900KS, 64GB RAM",
                    Price = 3499.99m,
                    Stock = 15,
                    CategoryId = pcCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1587831990711-23dfb25ce31d?w=500"
                },
                new Product
                {
                    Name = "Corsair Streaming Build PC",
                    Description = "Professional streaming setup with RTX 4070, Ryzen 7 7700X, 32GB RAM",
                    Price = 1999.99m,
                    Stock = 20,
                    CategoryId = pcCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1598327105666-5b89351aff97?w=500"
                },
                new Product
                {
                    Name = "Alienware Aurora R15",
                    Description = "Premium gaming desktop, RTX 4080, Intel i9, custom RGB lighting",
                    Price = 2799.99m,
                    Stock = 10,
                    CategoryId = pcCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1616763355603-9755a6ceb0f0?w=500"
                },
                // Laptops
                new Product
                {
                    Name = "MacBook Pro 16\" M4 Max",
                    Description = "Powerful laptop with M4 Max chip, 36GB unified memory, stunning display",
                    Price = 3499.99m,
                    Stock = 25,
                    CategoryId = laptopCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=500"
                },
                new Product
                {
                    Name = "Dell XPS 15 OLED",
                    Description = "Premium Windows laptop with RTX 4070, Intel i9, 4K OLED display",
                    Price = 2499.99m,
                    Stock = 18,
                    CategoryId = laptopCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1588872657840-790ff3bde4c5?w=500"
                },
                new Product
                {
                    Name = "ASUS ROG Zephyrus Gaming Laptop",
                    Description = "Gaming laptop with RTX 4090, Intel i9-13980HX, 240Hz display",
                    Price = 3299.99m,
                    Stock = 12,
                    CategoryId = laptopCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=500"
                },
                new Product
                {
                    Name = "Lenovo ThinkPad X1 Carbon",
                    Description = "Business ultrabook with Intel i7, 16GB RAM, lightweight and portable",
                    Price = 1599.99m,
                    Stock = 30,
                    CategoryId = laptopCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1571544784c4-c819c6935e56?w=500"
                },
                // Gaming Consoles
                new Product
                {
                    Name = "PlayStation 5 Pro",
                    Description = "Latest PS5 Pro with advanced graphics and AI upscaling",
                    Price = 799.99m,
                    Stock = 35,
                    CategoryId = consoleCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1606841836239-c5a2a2035d78?w=500"
                },
                new Product
                {
                    Name = "Xbox Series X",
                    Description = "Most powerful gaming console with 4K gaming and Game Pass",
                    Price = 499.99m,
                    Stock = 40,
                    CategoryId = consoleCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1633591941275-e0eea5e7b9b7?w=500"
                },
                new Product
                {
                    Name = "Nintendo Switch OLED",
                    Description = "Portable gaming console with OLED display, docking station included",
                    Price = 349.99m,
                    Stock = 50,
                    CategoryId = consoleCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1609708536965-3eb88aad0067?w=500"
                },
                // Handheld Devices
                new Product
                {
                    Name = "Steam Deck OLED 1TB",
                    Description = "Handheld PC gaming device with OLED display, plays full PC games",
                    Price = 649.99m,
                    Stock = 28,
                    CategoryId = handheldCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1611532736579-6b16e2b50449?w=500"
                },
                new Product
                {
                    Name = "Nintendo Switch Pro",
                    Description = "Advanced controller with gyro and HD rumble for Switch gaming",
                    Price = 69.99m,
                    Stock = 60,
                    CategoryId = handheldCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1602394247929-fbdf5cba27ad?w=500"
                },
                new Product
                {
                    Name = "Samsung Galaxy Tab S10 Ultra",
                    Description = "13.6\" tablet with AMOLED display, powerful processor, S Pen included",
                    Price = 1499.99m,
                    Stock = 22,
                    CategoryId = handheldCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1561070791-2526d30994b5?w=500"
                },
                // Monitors & Peripherals
                new Product
                {
                    Name = "ASUS ROG Swift OLED 4K Monitor",
                    Description = "32\" 4K 240Hz OLED gaming monitor with perfect colors",
                    Price = 1999.99m,
                    Stock = 8,
                    CategoryId = peripheralCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1511707267537-b85faf00021e?w=500"
                },
                new Product
                {
                    Name = "Logitech MX Master 3S Mouse",
                    Description = "Premium wireless mouse with advanced precision and customization",
                    Price = 99.99m,
                    Stock = 100,
                    CategoryId = peripheralCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1527814050087-3793815479db?w=500"
                },
                new Product
                {
                    Name = "Corsair K95 Platinum XT Keyboard",
                    Description = "Mechanical gaming keyboard with Cherry MX switches, RGB lighting",
                    Price = 229.99m,
                    Stock = 45,
                    CategoryId = peripheralCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1587829191301-e8fa46a9977d?w=500"
                },
                // Components & Upgrades
                new Product
                {
                    Name = "NVIDIA RTX 4090",
                    Description = "Ultra-high-end graphics card for extreme gaming and content creation",
                    Price = 1599.99m,
                    Stock = 12,
                    CategoryId = componentCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1587829191301-e8fa46a9977d?w=500"
                },
                new Product
                {
                    Name = "Intel Core i9-13900KS",
                    Description = "Flagship CPU with 24 cores, up to 6.2GHz boost clock",
                    Price = 699.99m,
                    Stock = 20,
                    CategoryId = componentCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1587829191301-e8fa46a9977d?w=500"
                },
                new Product
                {
                    Name = "Crucial P5 Plus NVMe SSD 2TB",
                    Description = "Fast NVMe SSD with 6600MB/s read speed, great for gaming",
                    Price = 199.99m,
                    Stock = 80,
                    CategoryId = componentCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1587829191301-e8fa46a9977d?w=500"
                },
                new Product
                {
                    Name = "Corsair Vengeance DDR5 32GB Kit",
                    Description = "High-speed DDR5 RAM for maximum performance",
                    Price = 149.99m,
                    Stock = 60,
                    CategoryId = componentCategory.Id,
                    ImageUrl = "https://images.unsplash.com/photo-1587829191301-e8fa46a9977d?w=500"
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }
    }
}
