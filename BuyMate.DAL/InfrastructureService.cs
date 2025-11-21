using BuyMate.BLL.Contracts;
using BuyMate.BLL.Contracts.Repositories;
using BuyMate.BLL.Features.User;
using BuyMate.DAL.Repositories;
using BuyMate.Model.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

            services.AddIdentity<User, IdentityRole<Guid>>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = false;

                options.Lockout.MaxFailedAccessAttempts = 3;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.AllowedForNewUsers = true;

            }).AddEntityFrameworkStores<BuyMateDbContext>().AddDefaultTokenProviders(); ;


            /*
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var secretKey = configuration.GetValue<string>("SecretKey") ?? throw new InvalidOperationException("SecretKey not found in configuration.");
                var secretKeyInBytes = Encoding.ASCII.GetBytes(secretKey);
                var key = new SymmetricSecurityKey(secretKeyInBytes);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false, // Disable audience validation
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidIssuer = "BuyMate-BackEnd",
                };
                //Handle unauthorized and forbidden
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.Redirect("/User/Login");
                        return Task.CompletedTask;
                    },
                    OnForbidden = context =>
                    {
                        context.Response.Redirect("/Home/Error");
                        return Task.CompletedTask;
                    },

                    //Get JWT Token from Cookie
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["jwt_token"];
                        return Task.CompletedTask;
                    }
            

                };
            });
            */
            //Handle Routes
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/User/Login"; // redirect to login if not authorized
                options.AccessDeniedPath = "/Home/AccessDenied"; //redirect if access is denied
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            });

            // Dependency Injection
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IUserProfileService, UserProfileService>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductImageRepository, ProductImageRepository>();
            services.AddScoped<IProductService, BuyMate.BLL.Features.Product.ProductService>();



            //Roles
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireRole("Admin");
                });
            });

            return services;


        }
    }
}
