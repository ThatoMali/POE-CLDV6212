using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Services;
using ABCRetailers.Models;

namespace ABCRetailers.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IAzureStorageService _storage;
        public CustomerController(IAzureStorageService storage) => _storage = storage;

        public async Task<IActionResult> Index()
        {
            var customers = await _storage.GetAllEntitiesAsync<Customer>();
            return View(customers.OrderBy(c => c.Name).ThenBy(c => c.Surname).ToList());
        }

        [HttpGet]
        public IActionResult Create() => View(new Customer());

        [HttpPost]
        public async Task<IActionResult> Create(Customer model)
        {
            if (!ModelState.IsValid) return View(model);
            model.PartitionKey = nameof(Customer);
            model.RowKey = Guid.NewGuid().ToString();
            await _storage.AddEntityAsync(model);
            TempData["Message"] = "Customer created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var customer = await _storage.GetEntityAsync<Customer>(nameof(Customer), id);
            if (customer == null) return NotFound();
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Customer model)
        {
            if (!ModelState.IsValid) return View(model);
            await _storage.UpdateEntityAsync(model);
            TempData["Message"] = "Customer updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _storage.DeleteEntityAsync<Customer>(nameof(Customer), id);
            TempData["Message"] = "Customer deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}