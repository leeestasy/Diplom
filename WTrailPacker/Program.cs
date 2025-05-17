using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WTrailPacker.Models;
using WTrailPacker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<TrailPackerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddScoped<IFoodCalculationService>(provider =>
{
    var dbContext = provider.GetRequiredService<TrailPackerDbContext>();
    //var logger = provider.GetRequiredService<ILogger<FoodCalculationService>>();
    return new FoodCalculationService(dbContext/*, logger*/);
});

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
