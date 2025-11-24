using BuyMate.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    var context = serviceScope.ServiceProvider.GetService<BuyMateDbContext>();
    context?.Database.Migrate();
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Database migration failed.");
    // Decide: do not crash app; continue running.
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