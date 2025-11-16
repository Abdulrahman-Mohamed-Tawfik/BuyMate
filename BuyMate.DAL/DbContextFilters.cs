using BuyMate.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace BuyMate.DAL
{
    public partial class BuyMateDbContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Order>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<OrderItem>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(p => !p.IsDeleted);

        }
    }
}
