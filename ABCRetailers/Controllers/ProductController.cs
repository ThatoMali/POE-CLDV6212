using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Services;
using ABCRetailers.Models;

namespace ABCRetailers.Controllers
{
    public class ProductController : Controller
    {
        private readonly IAzureStorageService _storage;
        private readonly IWebHostEnvironment _env;

        public ProductController(IAzureStorageService storage, IWebHostEnvironment env)
        {
            _storage = storage;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _storage.GetAllEntitiesAsync<Product>();
            return View(products.OrderBy(p => p.ProductName).ToList());
        }

        [HttpGet]
        public IActionResult Create() => View(new Product());

        [HttpPost]
        public async Task<IActionResult> Create(Product model, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(model);

            if (image != null && image.Length > 0)
            {
                using var stream = image.OpenReadStream();
                model.ImageUrl = await _storage.UploadImageAsync(stream, image.FileName, image.ContentType);
            }
            model.PartitionKey = nameof(Product);
            model.RowKey = Guid.NewGuid().ToString();

            await _storage.AddEntityAsync(model);
            TempData["Message"] = "Product created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var product = await _storage.GetEntityAsync<Product>(nameof(Product), id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product model, IFormFile? image)
        {
            if (!ModelState.IsValid) return View(model);
            if (image != null && image.Length > 0)
            {
                using var stream = image.OpenReadStream();
                model.ImageUrl = await _storage.UploadImageAsync(stream, image.FileName, image.ContentType);
            }
            await _storage.UpdateEntityAsync(model);
            TempData["Message"] = "Product updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _storage.DeleteEntityAsync<Product>(nameof(Product), id);
            TempData["Message"] = "Product deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}