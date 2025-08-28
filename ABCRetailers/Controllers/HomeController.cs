using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Services;
using ABCRetailers.Models;
using ABCRetailers.Models.ViewModels;

namespace ABCRetailers.Controllers
{
    public class HomeController : Controller
    {
        private readonly IAzureStorageService _storage;
        public HomeController(IAzureStorageService storage) => _storage = storage;

        public async Task<IActionResult> Index()
        {
            var customers = await _storage.GetAllEntitiesAsync<Customer>();
            var products  = await _storage.GetAllEntitiesAsync<Product>();
            var orders    = await _storage.GetAllEntitiesAsync<Order>();

            var vm = new HomeViewModel
            {
                FeaturedProducts = products.Take(5),
                CustomerCount = customers.Count,
                ProductCount = products.Count,
                OrderCount = orders.Count
            };
            return View(vm);
        }

        public IActionResult Privacy() => View();

        // Serve media from App_Data/uploads via a lightweight passthrough
        [HttpGet("/media/{file}")]
        public IActionResult Media(string file)
        {
            var dataRoot = Path.Combine(AppContext.BaseDirectory, "App_Data", "uploads");
            var path = Path.Combine(dataRoot, file);
            if (!System.IO.File.Exists(path)) return NotFound();
            return PhysicalFile(path, "application/octet-stream");
        }
    }
}