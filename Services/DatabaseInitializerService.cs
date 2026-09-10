using Final_Efstathiadis_Theodors.Data;
using Final_Efstathiadis_Theodors.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Final_Efstathiadis_Theodors.Services
{
    public class DatabaseInitializerService
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. Seed Roles
            string[] roleNames = { "Admin", "Customer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Default Admin User
            var adminEmail = "admin@coredeck.local";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "CoreDeck",
                    LastName = "Administrator",
                    EmailConfirmed = true,
                    Address = "Cyber Deck 1",
                    City = "Athens",
                    PostalCode = "10431",
                    Country = "Greece",
                    CreatedDate = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(adminUser, "Admin123!");
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed or Update Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category
                    {
                        Name = "PCs & Desktops",
                        Description = "High-performance battlestations and enthusiast gaming rigs",
                        IconUrl = "fas fa-desktop"
                    },
                    new Category
                    {
                        Name = "Laptops",
                        Description = "Ultra-portable performance laptops and mobile workstations",
                        IconUrl = "fas fa-laptop"
                    },
                    new Category
                    {
                        Name = "Gaming Consoles",
                        Description = "Next-gen consoles, VR kits, and competitive hardware",
                        IconUrl = "fas fa-gamepad"
                    },
                    new Category
                    {
                        Name = "Handheld Devices",
                        Description = "Portable gaming decks and high-power handheld devices",
                        IconUrl = "fas fa-mobile-alt"
                    },
                    new Category
                    {
                        Name = "Monitors & Peripherals",
                        Description = "High-refresh OLED monitors, mechanical switches, and esports mice",
                        IconUrl = "fas fa-tv"
                    },
                    new Category
                    {
                        Name = "Components & Upgrades",
                        Description = "Enthusiast GPUs, multicore CPUs, Gen5 NVMe SSDs, and DDR5 RAM",
                        IconUrl = "fas fa-microchip"
                    }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();

                var pcCategory = categories[0];
                var laptopCategory = categories[1];
                var consoleCategory = categories[2];
                var handheldCategory = categories[3];
                var peripheralCategory = categories[4];
                var componentCategory = categories[5];

                // 4. Seed Products with authentic hardware photography
                var products = new List<Product>
                {
                    // PCs & Desktops
                    new Product
                    {
                        Name = "CoreDeck Titan RTX 4090 Battlestation",
                        Description = "Flagship liquid-cooled gaming PC featuring RTX 4090 24GB, Intel Core i9-14900KS, 64GB DDR5-6000, 4TB Gen4 NVMe, and custom cyber-cyan chassis cabling.",
                        Price = 3899.99m,
                        Stock = 8,
                        CategoryId = pcCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1587202372775-e229f172b9d7?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Corsair Obsidian Stream Machine",
                        Description = "Dual-system dedicated streaming and gaming build with RTX 4070 Ti Super, AMD Ryzen 7 7800X3D, 32GB low-latency DDR5, and Elgato 4K capture card integrated.",
                        Price = 2299.99m,
                        Stock = 14,
                        CategoryId = pcCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1587831990711-23dfb25ce31d?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Alienware Horizon Neo Gaming Rig",
                        Description = "Precision-engineered dark graphite gaming rig with RTX 4080 Super, Intel Core i7-14700K, 32GB RGB RAM, 360mm AIO liquid cooling, and customizable alien aura lighting.",
                        Price = 2899.99m,
                        Stock = 6,
                        CategoryId = pcCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1616763355603-9755a6ceb0f0?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Laptops
                    new Product
                    {
                        Name = "MacBook Pro 16\" M4 Max Space Black",
                        Description = "Apple Silicon powerhouse with 16-core CPU, 40-core GPU, 48GB unified memory, 1TB SSD, Liquid Retina XDR display, and up to 24-hour battery endurance.",
                        Price = 3699.99m,
                        Stock = 12,
                        CategoryId = laptopCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Dell XPS 16 OLED Titanium",
                        Description = "Edge-to-edge 4K OLED InfinityEdge laptop, GeForce RTX 4070 8GB, Intel Core Ultra 9, 32GB LPDDR5x, graphite machined aluminum with carbon fiber palm rest.",
                        Price = 2699.99m,
                        Stock = 10,
                        CategoryId = laptopCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1588872657840-790ff3bde4c5?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "ASUS ROG Zephyrus G16 Cyber-Chassis",
                        Description = "Ultra-slim 1.8cm CNC aluminum gaming laptop, RTX 4090, Intel Core Ultra 9, 240Hz ROG Nebula OLED display with G-Sync support, 32GB RAM.",
                        Price = 3399.99m,
                        Stock = 7,
                        CategoryId = laptopCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Gaming Consoles
                    new Product
                    {
                        Name = "Sony PlayStation 5 Pro 2TB",
                        Description = "Next-level 4K 60FPS console gaming with advanced ray tracing hardware, PlayStation Spectral Super Resolution AI upscaling, and 2TB ultra-fast NVMe storage.",
                        Price = 799.99m,
                        Stock = 25,
                        CategoryId = consoleCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1606841836239-c5a2a2035d78?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Microsoft Xbox Series X Cyber Edition",
                        Description = "12 teraflops raw graphical computing power, true 4K gaming, Quick Resume for multiple titles, 1TB Custom NVMe SSD with ultra-quiet vapor chamber cooling.",
                        Price = 499.99m,
                        Stock = 30,
                        CategoryId = consoleCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1633591941275-e0eea5e7b9b7?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Handheld Devices
                    new Product
                    {
                        Name = "Steam Deck OLED 1TB Cyber Black",
                        Description = "Handheld enthusiast PC with 7.4\" 90Hz HDR OLED display, custom AMD APU, 50Wh battery, premium anti-glare etched glass, and full Steam library compatibility.",
                        Price = 649.99m,
                        Stock = 22,
                        CategoryId = handheldCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1612287233207-64010b9380ef?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "ASUS ROG Ally X Handheld Console",
                        Description = "Extreme handheld power with AMD Ryzen Z1 Extreme, 24GB LPDDR5X-7500 RAM, 80Wh monster battery, ergonomic grips, and dual USB-C ports with Thunderbolt support.",
                        Price = 799.99m,
                        Stock = 18,
                        CategoryId = handheldCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1602394247929-fbdf5cba27ad?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Monitors & Peripherals
                    new Product
                    {
                        Name = "ASUS ROG Swift OLED 32\" 4K 240Hz Gaming Monitor",
                        Description = "The ultimate display: 32-inch 4K QD-OLED panel, blistering 240Hz refresh rate, 0.03ms response time, custom heatsink design, and true 99% DCI-P3 color gamut.",
                        Price = 1299.99m,
                        Stock = 9,
                        CategoryId = peripheralCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Corsair K95 RGB Platinum XT Mechanical Keyboard",
                        Description = "Aircraft-grade anodized aluminum frame, Cherry MX Speed Silver switches, dynamic per-key RGB backlighting with 19-zone LightEdge, and dedicated macro keys.",
                        Price = 219.99m,
                        Stock = 35,
                        CategoryId = peripheralCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1587829191301-e8fa46a9977d?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Logitech G PRO X Superlight 2 Wireless Mouse",
                        Description = "Pro-grade esports mouse weighing just 60 grams, HERO 2 sensor with 32,000 DPI, LIGHTFORCE hybrid optical-mechanical switches, and 95 hours continuous battery life.",
                        Price = 159.99m,
                        Stock = 45,
                        CategoryId = peripheralCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1527814050087-3793815479db?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },

                    // Components & Upgrades (CPUs, GPUs, RAM, SSDs)
                    new Product
                    {
                        Name = "NVIDIA GeForce RTX 4090 Founders Edition 24GB",
                        Description = "Ada Lovelace architecture king with 16,384 CUDA cores, 24GB GDDR6X 384-bit memory, DLSS 3 frame generation, and extreme 4K 144Hz ray tracing performance.",
                        Price = 1749.99m,
                        Stock = 5,
                        CategoryId = componentCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1591488320449-011701bb6704?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Intel Core i9-14900KS 24-Core Processor",
                        Description = "Special Edition desktop processor reaching up to 6.2GHz thermal velocity boost clock out of the box, 24 cores (8 P-cores + 16 E-cores), 32 threads, 36MB Intel Smart Cache.",
                        Price = 699.99m,
                        Stock = 12,
                        CategoryId = componentCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1591799264318-7e6ef8ddb7ea?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Samsung 990 PRO 2TB PCIe 4.0 NVMe SSD",
                        Description = "Sequential read speeds up to 7,450 MB/s, proprietary nickel-coated thermal controller, optimized power efficiency, ideal for intense 4K video editing and PS5 consoles.",
                        Price = 189.99m,
                        Stock = 50,
                        CategoryId = componentCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Corsair Dominator Titanium DDR5 64GB (2x32GB) 6000MHz",
                        Description = "Enthusiast DDR5 memory kit with forged aluminum construction, patented DHX cooling technology, vibrant 11-zone CAPELLIX RGB LEDs, and Intel XMP 3.0 support.",
                        Price = 289.99m,
                        Stock = 28,
                        CategoryId = componentCategory.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1562976540-1502c2145186?w=800&auto=format&fit=crop&q=80",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
            else
            {
                // Fix any existing category icon URLs and product keyboard placeholder image URLs
                var existingCategories = await context.Categories.ToListAsync();
                foreach (var cat in existingCategories)
                {
                    if (string.IsNullOrWhiteSpace(cat.IconUrl) || cat.IconUrl.Contains("flaticon"))
                    {
                        cat.IconUrl = cat.Name switch
                        {
                            "PCs & Desktops" => "fas fa-desktop",
                            "Laptops" => "fas fa-laptop",
                            "Gaming Consoles" => "fas fa-gamepad",
                            "Handheld Devices" => "fas fa-mobile-alt",
                            "Monitors & Peripherals" => "fas fa-tv",
                            "Components & Upgrades" => "fas fa-microchip",
                            _ => "fas fa-tag"
                        };
                    }
                }

                var existingProducts = await context.Products.ToListAsync();
                foreach (var prod in existingProducts)
                {
                    prod.IsActive = true;
                    // Fix products that were incorrectly given keyboard photo
                    if (!string.IsNullOrEmpty(prod.ImageUrl) && prod.ImageUrl.Contains("1587829191301-e8fa46a9977d"))
                    {
                        var name = prod.Name ?? "";
                        if (name.Contains("RTX", StringComparison.OrdinalIgnoreCase))
                        {
                            prod.ImageUrl = "https://images.unsplash.com/photo-1591488320449-011701bb6704?w=800&auto=format&fit=crop&q=80";
                        }
                        else if (name.Contains("Intel", StringComparison.OrdinalIgnoreCase) || name.Contains("CPU", StringComparison.OrdinalIgnoreCase))
                        {
                            prod.ImageUrl = "https://images.unsplash.com/photo-1591799264318-7e6ef8ddb7ea?w=800&auto=format&fit=crop&q=80";
                        }
                        else if (name.Contains("SSD", StringComparison.OrdinalIgnoreCase))
                        {
                            prod.ImageUrl = "https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?w=800&auto=format&fit=crop&q=80";
                        }
                        else if (name.Contains("RAM", StringComparison.OrdinalIgnoreCase) || name.Contains("DDR", StringComparison.OrdinalIgnoreCase))
                        {
                            prod.ImageUrl = "https://images.unsplash.com/photo-1562976540-1502c2145186?w=800&auto=format&fit=crop&q=80";
                        }
                    }
                }

                await context.SaveChangesAsync();
            }
        }
    }
}
