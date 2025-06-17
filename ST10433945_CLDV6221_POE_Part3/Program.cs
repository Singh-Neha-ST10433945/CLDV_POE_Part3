using Microsoft.EntityFrameworkCore;
using EventEaseBookingSystem.Models;
using EventEaseBookingSystem.Services; // ✅ Include this to access IAzureBlobStorageService

var builder = WebApplication.CreateBuilder(args);

// Hardcoded connection string
var connectionString = "Server=tcp:cldv-eventeasebookingsystem.database.windows.net,1433;Initial Catalog=EventEaseBookingSystem;Persist Security Info=False;User ID=NehaSingh;Password=Misty@16;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

// Add services
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// ✅ Register Azure Blob Storage service
builder.Services.AddScoped<IAzureBlobStorageService, AzureBlobStorageService>();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
