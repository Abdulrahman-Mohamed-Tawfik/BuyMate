using BuyMate.DAL;
using BuyMate.Model.Entities;
using Microsoft.AspNetCore.Identity;

namespace BuyMate.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var userManager = services.GetRequiredService<UserManager<BuyMate.Model.Entities.User>>();
            var context = services.GetRequiredService<BuyMateDbContext>();
            var config = services.GetRequiredService<IConfiguration>();

            await EnsureRoleExists(roleManager, "admin");
            await EnsureRoleExists(roleManager, "user");

            await EnsureAdminUserAsync(userManager, config);
            await EnsureProductsExist(context);
        }

     
        private static async Task EnsureRoleExists(RoleManager<IdentityRole<Guid>> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var role = new IdentityRole<Guid>(roleName);
                var result = await roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors);
                    throw new InvalidOperationException($"Failed to create role '{roleName}': {errors}");
                }
            }
        }

        private static async Task EnsureAdminUserAsync(UserManager<BuyMate.Model.Entities.User> userManager, IConfiguration config)
        {
            var email = config["SeedAdmin:Email"];
            var password = config["SeedAdmin:Password"];
            var phone = config["SeedAdmin:Phone"] ?? string.Empty;
            var firstName = config["SeedAdmin:FirstName"] ?? "Admin";
            var lastName = config["SeedAdmin:LastName"] ?? "User";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return; // Do not seed without secure configured credentials
            }

            var existing = await userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                if (!await userManager.IsInRoleAsync(existing, "admin"))
                {
                    await userManager.AddToRoleAsync(existing, "admin");
                }
                return;
            }

            var user = new BuyMate.Model.Entities.User
            {
                UserName = BuildUserNameFromEmail(email),
                Email = email,
                PhoneNumber = phone,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(phone)
            };

            var create = await userManager.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                var errors = string.Join("; ", create.Errors);
                throw new InvalidOperationException($"Failed to create admin user: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(user, "admin");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors);
                throw new InvalidOperationException($"Admin user created but role assignment failed: {errors}");
            }
        }

        private static async Task EnsureProductsExist(BuyMateDbContext context)
        {
            if (context.Products.Any())
            {
                return; // DB has been seeded
            }
            // Ensure categories exist
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
               {
                   new Category { Name = "Electronics", ImageUrl = "/images/categories/electronics.jpg" },
                   new Category { Name = "Computers & Accessories", ImageUrl = "/images/categories/computers.jpg" },
                   new Category { Name = "Home & Kitchen", ImageUrl = "/images/categories/home_kitchen.jpg" },
                   new Category { Name = "Fashion", ImageUrl = "/images/categories/fashion.jpg" },
                   new Category { Name = "Sports & Outdoors", ImageUrl = "/images/categories/sports.jpg" },
                   new Category { Name = "Books", ImageUrl = "/images/categories/books.jpg" },
                   new Category { Name = "Beauty & Personal Care", ImageUrl = "/images/categories/beauty.jpg" },
                   new Category { Name = "Toys & Games", ImageUrl = "/images/categories/toys.jpg" },
                   new Category { Name = "Health & Personal Care", ImageUrl = "/images/categories/health.jpg" },
                   new Category { Name = "Baby & Kids", ImageUrl = "/images/categories/baby.jpg" }
               };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }
            var categoriesLookup = context.Categories.ToList();
            var rnd = new Random(1337);
            var products = new List<Product>
           {
               new Product
               {
                   Name = "EchoTech Pulse Bluetooth Speaker",
                   Description = "Portable Bluetooth speaker with rich bass, IPX6 water resistance, and 12-hour battery life. Perfect for indoor and outdoor use.",
                   Price = 79.99M,
                   DiscountPercentage = 10,
                   StockQuantity = rnd.Next(10, 200),
                   Brand = "EchoTech",
                   ProductCategories = new List<ProductCategory>
                   {
                       new ProductCategory { CategoryId = categoriesLookup.First(c => c.Name == "Electronics").Id }
                   },
                   Images = new List<ProductImage>
                   {
                       new ProductImage { ImageUrl = "/images/products/echo_pulse_1.jpg", IsMain = true },
                       new ProductImage { ImageUrl = "/images/products/echo_pulse_2.jpg" }
                   },
                   ProductSpecifications = new List<ProductSpecification>
                   {
                       new ProductSpecification { Key = "Battery Life", Value = "12 hours" },
                       new ProductSpecification { Key = "Water Resistance", Value = "IPX6" },
                       new ProductSpecification { Key = "Connectivity", Value = "Bluetooth 5.2" }
                   }
               },
               new Product
               {
                   Name = "Nova X15 Portable SSD 1TB",
                   Description = "High-performance external SSD with USB-C 3.2 Gen2 for fast file transfers and compact aluminium enclosure.",
                   Price = 129.50M,
                   DiscountPercentage = 5,
                   StockQuantity = rnd.Next(5, 150),
                   Brand = "Nova",
                   ProductCategories = new List<ProductCategory>
                   {
                       new ProductCategory { CategoryId = categoriesLookup.First(c => c.Name == "Computers & Accessories").Id }
                   },
                   Images = new List<ProductImage>
                   {
                       new ProductImage { ImageUrl = "/images/products/nova_x15_1.jpg", IsMain = true },
                       new ProductImage { ImageUrl = "/images/products/nova_x15_2.jpg" }
                   },
                   ProductSpecifications = new List<ProductSpecification>
                   {
                       new ProductSpecification { Key = "Capacity", Value = "1 TB" },
                       new ProductSpecification { Key = "Interface", Value = "USB-C 3.2 Gen2" },
                       new ProductSpecification { Key = "Read Speed", Value = "Up to 1050 MB/s" }
                   }
               },
               new Product
               {
                   Name = "HomeEase 6-Quart Air Fryer",
                   Description = "6-quart digital air fryer with 8 cooking presets, non-stick basket, and easy-to-clean design. Healthier frying with less oil.",
                   Price = 99.00M,
                   DiscountPercentage = 15,
                   StockQuantity = rnd.Next(20, 120),
                   Brand = "HomeEase",
                   ProductCategories = new List<ProductCategory>
                   {
                       new ProductCategory { CategoryId = categoriesLookup.First(c => c.Name == "Home & Kitchen").Id }
                   },
                   Images = new List<ProductImage>
                   {
                       new ProductImage { ImageUrl = "/images/products/homeease_airfryer_1.jpg", IsMain = true },
                       new ProductImage { ImageUrl = "/images/products/homeease_airfryer_2.jpg" }
                   },
                   ProductSpecifications = new List<ProductSpecification>
                   {
                       new ProductSpecification { Key = "Capacity", Value = "6 Quart" },
                       new ProductSpecification { Key = "Preset Modes", Value = "8" },
                       new ProductSpecification { Key = "Power", Value = "1500W" }
                   }
               },
               new Product
               {
                   Name = "ModaLux Men's Classic Oxford Shirt",
                   Description = "Tailored classic oxford shirt made from 100% premium cotton. Machine washable and wrinkle resistant finish.",
                   Price = 45.00M,
                   DiscountPercentage = 0,
                   StockQuantity = rnd.Next(5, 300),
                   Brand = "ModaLux",
                   ProductCategories = new List<ProductCategory>
                   {
                       new ProductCategory { CategoryId = categoriesLookup.First(c => c.Name == "Fashion").Id }
                   },
                   Images = new List<ProductImage>
                   {
                       new ProductImage { ImageUrl = "/images/products/modalux_oxford_1.jpg", IsMain = true },
                       new ProductImage { ImageUrl = "/images/products/modalux_oxford_2.jpg" }
                   },
                   ProductSpecifications = new List<ProductSpecification>
                   {
                       new ProductSpecification { Key = "Material", Value = "100% Cotton" },
                       new ProductSpecification { Key = "Fit", Value = "Regular" },
                       new ProductSpecification { Key = "Care", Value = "Machine wash cold" }
                   }
               },
               new Product
               {
                   Name = "PeakGear Running Shoes",
                   Description = "Lightweight running shoes with breathable mesh upper and responsive foam midsole for daily training.",
                   Price = 74.99M,
                   DiscountPercentage = 20,
                   StockQuantity = rnd.Next(10, 250),
                   Brand = "PeakGear",
                   ProductCategories = new List<ProductCategory>
                   {
                       new ProductCategory { CategoryId = categoriesLookup.First(c => c.Name == "Sports & Outdoors").Id }
                   },
                   Images = new List<ProductImage>
                   {
                       new ProductImage { ImageUrl = "/images/products/peakgear_shoes_1.jpg", IsMain = true },
                       new ProductImage { ImageUrl = "/images/products/peakgear_shoes_2.jpg" }
                   },
                   ProductSpecifications = new List<ProductSpecification>
                   {
                       new ProductSpecification { Key = "Upper", Value = "Breathable mesh" },
                       new ProductSpecification { Key = "Sole", Value = "Rubber outsole" },
                       new ProductSpecification { Key = "Weight", Value = "280g (size 9)" }
                   }
               },
               new Product
               {
                   Name = "HarborPress: Modern Web Development (Paperback)",
                   Description = "Comprehensive guide to modern web development techniques, covering HTML5, CSS3, JavaScript ES2021 and backend APIs.",
                   Price = 34.95M,
                   DiscountPercentage = 0,
                   StockQuantity = rnd.Next(0, 1000),
                   Brand = "HarborPress",
                   ProductCategories = new List<ProductCategory>
                   {
                       new ProductCategory { CategoryId = categoriesLookup.First(c => c.Name == "Books").Id }
                   },
                   Images = new List<ProductImage>
                   {
                       new ProductImage { ImageUrl = "/images/products/modern_web_1.jpg", IsMain = true }
                   },
                   ProductSpecifications = new List<ProductSpecification>
                   {
                       new ProductSpecification { Key = "Pages", Value = "512" },
                       new ProductSpecification { Key = "Language", Value = "English" },
                       new ProductSpecification { Key = "ISBN", Value = "978-1-23456-789-7" }
                   }
               },
               new Product
               {
                   Name = "GlowWell Revitalizing Vitamin C Serum",
                   Description = "Lightweight serum with stabilized Vitamin C to brighten skin tone and reduce fine lines. Suitable for all skin types.",
                   Price = 24.50M,
                   DiscountPercentage = 10,
                   StockQuantity = rnd.Next(5, 400),
                   Brand = "GlowWell",
                   ProductCategories = new List<ProductCategory>
                   {
                       new ProductCategory { CategoryId = categoriesLookup.First(c => c.Name == "Beauty & Personal Care").Id }
                   },
                   Images = new List<ProductImage>
                   {
                       new ProductImage { ImageUrl = "/images/products/glowwell_serum_1.jpg", IsMain = true },
                       new ProductImage { ImageUrl = "/images/products/glowwell_serum_2.jpg" }
                   },
                   ProductSpecifications = new List<ProductSpecification>
                   {
                       new ProductSpecification { Key = "Volume", Value = "30 ml" },
                       new ProductSpecification { Key = "Active Ingredient", Value = "10% Vitamin C" },
                       new ProductSpecification { Key = "Skin Type", Value = "All" }
                   }
               },
               new Product
               {
                   Name = "PlayWorks Building Blocks Set - 300 pcs",
                   Description = "Creative building blocks kit for ages 4+. Includes colorful bricks and an instruction booklet to build multiple models.",
                   Price = 39.99M,
                   DiscountPercentage = 5,
                   StockQuantity = rnd.Next(10, 500),
                   Brand = "PlayWorks",
                   ProductCategories = new List<ProductCategory>
                   {
                       new ProductCategory { CategoryId = categoriesLookup.First(c => c.Name == "Toys & Games").Id }
                   },
                   Images = new List<ProductImage>
                   {
                       new ProductImage { ImageUrl = "/images/products/playworks_blocks_1.jpg", IsMain = true },
                       new ProductImage { ImageUrl = "/images/products/playworks_blocks_2.jpg" }
                   },
                   ProductSpecifications = new List<ProductSpecification>
                   {
                       new ProductSpecification { Key = "Pieces", Value = "300" },
                       new ProductSpecification { Key = "Recommended Age", Value = "4+" },
                       new ProductSpecification { Key = "Material", Value = "ABS Plastic" }
                   }
               },
               new Product
               {
                   Name = "BabyNest Convertible Stroller",
                   Description = "Lightweight convertible stroller with adjustable recline, sun canopy and large storage basket. Suitable from birth with included newborn insert.",
                   Price = 219.00M,
                   DiscountPercentage = 12,
                   StockQuantity = rnd.Next(2, 80),
                   Brand = "BabyNest",
                   ProductCategories = new List<ProductCategory>
                   {
                       new ProductCategory { CategoryId = categoriesLookup.First(c => c.Name == "Baby & Kids").Id }
                   },
                   Images = new List<ProductImage>
                   {
                       new ProductImage { ImageUrl = "/images/products/babynest_stroller_1.jpg", IsMain = true },
                       new ProductImage { ImageUrl = "/images/products/babynest_stroller_2.jpg" }
                   },
                   ProductSpecifications = new List<ProductSpecification>
                   {
                       new ProductSpecification { Key = "Weight Capacity", Value = "22 kg" },
                       new ProductSpecification { Key = "Frame Material", Value = "Aluminum" },
                       new ProductSpecification { Key = "Fold Type", Value = "One-hand fold" }
                   }
               }
           };
            context.Products.AddRange(products);
            context.SaveChanges();
        }

        private static string BuildUserNameFromEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return Guid.NewGuid().ToString();
            var parts = email.Split('@');
            return parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]) ? parts[0] : Guid.NewGuid().ToString();
        }
    }
}
