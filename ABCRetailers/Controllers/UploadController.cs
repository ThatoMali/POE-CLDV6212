using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Services;
using ABCRetailers.Models;

namespace ABCRetailers.Controllers
{
    public class UploadController : Controller
    {
        private readonly IAzureStorageService _storage;
        public UploadController(IAzureStorageService storage) => _storage = storage;

        [HttpGet]
        public IActionResult Index() => View(new FileUploadModel());

        [HttpPost]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (model.ProofOfPayment == null || model.ProofOfPayment.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please select a file to upload.");
                return View(model);
            }

            using var stream = model.ProofOfPayment.OpenReadStream();
            var saved = await _storage.UploadToFileShareAsync(stream, model.ProofOfPayment.FileName);
            await _storage.SendMessageAsync($"Uploaded contract/proof {saved} for Order {model.OrderId}, Customer {model.CustomerName}");

            TempData["Message"] = $"File uploaded as {saved}.";
            return RedirectToAction(nameof(Index));
        }
    }
}