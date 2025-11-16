using BuyMate.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BuyMate.DAL
{
    public partial class BuyMateDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public BuyMateDbContext(DbContextOptions<BuyMateDbContext> options)
        : base(options)
        {
        }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ProductReview> ProductReviews { get; set; }
       
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<WishlistItem> WishlistItems { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            // Do not create the Identity user tokens table; tokens will be stored on Users table
            builder.Ignore<IdentityUserToken<Guid>>();

            builder.Entity<User>(entity =>
            {
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.Property(u => u.Address).HasMaxLength(512);
                entity.Property(u => u.ProfileImageUrl).HasMaxLength(256);
                // Ensure proper SQL type for DateOnly
                entity.Property(u => u.BirthDate).HasColumnType("date");
            });

            // Cart
            builder.Entity<Cart>(entity =>
            {

                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.User)
                      .WithOne(u => u.Cart)
                      .HasForeignKey<Cart>(c => c.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(c => c.Items)
                      .WithOne(ci => ci.Cart)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);

            });

            // CartItem
            builder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.Id);

                entity.HasOne(ci => ci.Cart)
                      .WithMany(c => c.Items)
                      .HasForeignKey(ci => ci.CartId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ci => ci.Product)
                        .WithMany(p => p.CartItems)
                        .HasForeignKey(ci => ci.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);

                //a product can appear only once per cart
                entity.HasIndex(ci => new { ci.CartId, ci.ProductId })
                      .IsUnique();

            });

            //Category
            builder.Entity<Category>(entity =>
             {
                 entity.HasKey(c => c.Id);

                 entity.HasOne(c => c.ParentCategory)
                       .WithMany(c => c.SubCategories)
                       .HasForeignKey(c => c.ParentCategoryId)
                       .OnDelete(DeleteBehavior.Restrict);

                 entity.Property(c => c.Name).IsRequired().HasMaxLength(200);


             });


            //Product Category
            builder.Entity<ProductCategory>(entity =>
            {
                entity.HasKey(pc => pc.Id);

                entity.HasOne(pc => pc.Product)
                      .WithMany(p => p.ProductCategories)
                      .HasForeignKey(pc => pc.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pc => pc.Category)
                        .WithMany(c => c.ProductCategories)
                        .HasForeignKey(pc => pc.CategoryId)
                        .OnDelete(DeleteBehavior.Cascade);

                //a product can appear only once per category
                entity.HasIndex(pc => new { pc.ProductId, pc.CategoryId })
                      .IsUnique();

            });

            //Product Review
            builder.Entity<ProductReview>(entity =>
            {
                entity.HasKey(pr => pr.Id);

                entity.HasOne(pr => pr.Product)
                      .WithMany(p => p.Reviews)
                      .HasForeignKey(pr => pr.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pr => pr.User)
                        .WithMany(u => u.Reviews)
                        .HasForeignKey(pr => pr.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
                //a user can review a product only once
                entity.HasIndex(pr => new { pr.ProductId, pr.UserId })
                      .IsUnique();
                entity.Property(pr => pr.Rating).IsRequired();
                entity.Property(pr => pr.Review).IsRequired().HasMaxLength(4000);
            });

            //Wishlist Item
            builder.Entity<WishlistItem>(entity =>
            {
                entity.HasKey(wi => wi.Id);

                entity.HasOne(wi => wi.Wishlist)
                      .WithMany(w => w.Items)
                      .HasForeignKey(wi => wi.WishlistId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(wi => wi.Product)
                        .WithMany(p => p.WishlistItems)
                        .HasForeignKey(wi => wi.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);

                //a product can appear only once per wishlist
                entity.HasIndex(wi => new { wi.WishlistId, wi.ProductId })
                      .IsUnique();
            });

            //Order Item
            builder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);

                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.Items)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Product)
                        .WithMany(p => p.OrderItems)
                        .HasForeignKey(oi => oi.ProductId)
                        .OnDelete(DeleteBehavior.Restrict);



            });


            // filter configurations
            OnModelCreatingPartial(builder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
