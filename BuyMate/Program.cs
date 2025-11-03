using BuyMate.DAL;
using BuyMate.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// EF Core + Identity Core with SQL Server
builder.Services.AddDbContext<BuyMateDbContext>(options =>
{
    // Note: for demo, using a placeholder connection string name
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configure Identity to use the custom User entity with Guid keys
builder.Services.AddIdentityCore<User>(options =>
{
    // configure identity options if needed
})
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<BuyMateDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
