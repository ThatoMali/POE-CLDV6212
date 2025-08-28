using ABCRetailers.Models;

namespace ABCRetailers.Models.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Product> FeaturedProducts { get; set; } = new List<Product>();
        public int CustomerCount { get; set; }
        public int ProductCount { get; set; }
        public int OrderCount { get; set; }
    }
}
