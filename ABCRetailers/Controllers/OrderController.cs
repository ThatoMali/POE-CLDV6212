using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ABCRetailers.Services;
using ABCRetailers.Models;
using ABCRetailers.Models.ViewModels;

namespace ABCRetailers.Controllers
{
    public class OrderController : Controller
    {
        private readonly IAzureStorageService _storage;
        public OrderController(IAzureStorageService storage) => _storage = storage;

        public async Task<IActionResult> Index()
        {
            var orders = await _storage.GetAllEntitiesAsync<Order>();
            return View(orders.OrderByDescending(o => o.OrderDate).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new OrderCreateViewModel();
            await PopulateDropdowns(vm);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateViewModel vm)
        {
            if (vm.CustomerId == null) ModelState.AddModelError(nameof(vm.CustomerId), "Select a customer.");
            if (vm.ProductId == null) ModelState.AddModelError(nameof(vm.ProductId), "Select a product.");
            if (!ModelState.IsValid) { await PopulateDropdowns(vm); return View(vm); }

            var product = (await _storage.GetAllEntitiesAsync<Product>()).First(p => p.RowKey == vm.ProductId);
            var customer = (await _storage.GetAllEntitiesAsync<Customer>()).First(c => c.RowKey == vm.CustomerId);

            var order = new Order
            {
                PartitionKey = nameof(Order),
                RowKey = Guid.NewGuid().ToString(),
                OrderId = Guid.NewGuid().ToString("N").Substring(0,8).ToUpperInvariant(),
                CustomerId = customer.RowKey,
                Username = customer.Username,
                ProductId = product.RowKey,
                ProductName = product.ProductName,
                OrderDate = vm.OrderDate,
                Quantity = vm.Quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * vm.Quantity,
                Status = vm.Status
            };

            await _storage.AddEntityAsync(order);
            await _storage.SendMessageAsync($"Order {order.OrderId} placed for {customer.Username} x{order.Quantity} {product.ProductName}");

            TempData["Message"] = "Order created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var order = await _storage.GetEntityAsync<Order>(nameof(Order), id);
            if (order == null) return NotFound();

            var vm = new OrderCreateViewModel
            {
                CustomerId = order.CustomerId,
                ProductId = order.ProductId,
                Quantity = order.Quantity,
                OrderDate = order.OrderDate,
                Status = order.Status,
                UnitPrice = order.UnitPrice,
                TotalPrice = order.TotalPrice
            };
            await PopulateDropdowns(vm);
            ViewBag.RowKey = order.RowKey;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, OrderCreateViewModel vm)
        {
            if (vm.CustomerId == null) ModelState.AddModelError(nameof(vm.CustomerId), "Select a customer.");
            if (vm.ProductId == null) ModelState.AddModelError(nameof(vm.ProductId), "Select a product.");
            if (!ModelState.IsValid) { await PopulateDropdowns(vm); ViewBag.RowKey = id; return View(vm); }

            var order = await _storage.GetEntityAsync<Order>(nameof(Order), id);
            if (order == null) return NotFound();

            var product = (await _storage.GetAllEntitiesAsync<Product>()).First(p => p.RowKey == vm.ProductId);
            var customer = (await _storage.GetAllEntitiesAsync<Customer>()).First(c => c.RowKey == vm.CustomerId);

            order.CustomerId = customer.RowKey;
            order.Username = customer.Username;
            order.ProductId = product.RowKey;
            order.ProductName = product.ProductName;
            order.OrderDate = vm.OrderDate;
            order.Quantity = vm.Quantity;
            order.UnitPrice = product.Price;
            order.TotalPrice = product.Price * vm.Quantity;
            order.Status = vm.Status;

            await _storage.UpdateEntityAsync(order);
            TempData["Message"] = "Order updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            await _storage.DeleteEntityAsync<Order>(nameof(Order), id);
            TempData["Message"] = "Order deleted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ProductInfo(string id)
        {
            var product = (await _storage.GetAllEntitiesAsync<Product>()).FirstOrDefault(p => p.RowKey == id);
            return Json(new { price = product?.Price ?? 0m, stock = product?.StockAvailable ?? 0 });
        }

        private async Task PopulateDropdowns(OrderCreateViewModel vm)
        {
            var customers = await _storage.GetAllEntitiesAsync<Customer>();
            vm.Customers = customers
                .Select(c => new SelectListItem { Value = c.RowKey, Text = $"{c.Name} {c.Surname} ({c.Username})" })
                .ToList();

            var products = await _storage.GetAllEntitiesAsync<Product>();
            vm.Products = products
                .Select(p => new SelectListItem { Value = p.RowKey, Text = $"{p.ProductName} - R{p.Price}" })
                .ToList();
        }
    }
}