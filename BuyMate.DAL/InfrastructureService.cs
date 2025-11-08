using BuyMate.BLL.Contracts;
using BuyMate.DAL.Repositories;
using BuyMate.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuyMate.DAL
{
    public static class InfrastructureService
    {
        public static IServiceCollection AddInfrastructureService(this IServiceCollection services, IConfiguration configuration)
        {
            // EF Core + SQL Server
            services.AddDbContext<BuyMateDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            // Identity Core with Guid keys
            services.AddIdentityCore<User>(options =>
            {
                // configure identity options if needed
            })
            .AddRoles<IdentityRole<System.Guid>>()
            .AddEntityFrameworkStores<BuyMateDbContext>();

            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();

            return services;
        }
    }
}
