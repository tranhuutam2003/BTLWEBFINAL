using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using TestWeb.Data;
using TestWeb.Models;
using TestWeb.Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("BookContext")));

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddHostedService<OrderStatusUpdateService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession();

builder.Services.AddSignalR();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapHub<OrderHub>("/orderHub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
