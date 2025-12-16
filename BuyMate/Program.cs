using BuyMate.DAL;
using Microsoft.EntityFrameworkCore;
using BuyMate.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Centralized DI
builder.Services.AddInfrastructureService(builder.Configuration);

builder.Services.AddHttpClient("api", client =>
{
    client.BaseAddress = new Uri("https://localhost:7233/");
});
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

// Configure Authentication: JWT Bearer for web + API using same secret key
var secretKey = builder.Configuration["SecretKey"] ?? string.Empty;
var keyBytes = Encoding.UTF8.GetBytes(secretKey);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.FromMinutes(2)
        };

        // Read token from HTTP-only cookie for MVC/Razor requests
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("AuthToken", out var token) && !string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

// Global exception event handlers (capture crashes)
AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
{
    try
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(args.ExceptionObject as Exception, "Unhandled domain exception - process may terminate.");
    }
    catch { /* ignore logging failures */ }
};

TaskScheduler.UnobservedTaskException += (sender, args) =>
{
    try
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogCritical(args.Exception, "Unobserved task exception.");
        args.SetObserved();
    }
    catch { }
};

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Safe database migration with logging to avoid silent crash
try
{
    using var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    var services = serviceScope.ServiceProvider;
    var context = services.GetService<BuyMateDbContext>();

    // Increase migration timeout just for startup migration
    context!.Database.SetCommandTimeout(TimeSpan.FromMinutes(5));

    // Apply migrations
    await context.Database.MigrateAsync();
    // Seed initial data
    await DataSeeder.SeedAsync(services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Database migration or seeding failed - application startup aborted.");
    throw; // re throw to prevent app from starting
}

// Global exception logging middleware (request pipeline)
app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = ctx.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Unhandled request exception.");
        throw; // let developer page / exception handler process it
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();