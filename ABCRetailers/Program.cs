using ABCREtailers.Extensions;
using ABCRetailers.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Storage service (local, file-based fallback that requires no Azure)
builder.Services.AddSingleton<IAzureStorageService>(sp =>
{
    var cfg = builder.Configuration.GetSection("AzureStorage");
    var dataRoot = Path.Combine(AppContext.BaseDirectory, "App_Data");
    Directory.CreateDirectory(dataRoot);
    return new AzureStorageService(dataRoot);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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