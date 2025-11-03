using BuyMate.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BuyMate.DAL
{
    public class BuyMateDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public BuyMateDbContext(DbContextOptions<BuyMateDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>().ToTable("Users");
            builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
            builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
            builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
            builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
            // Do not create the Identity user tokens table; tokens will be stored on Users
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
        }
    }
}
