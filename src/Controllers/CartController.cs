using Microsoft.AspNetCore.Mvc;
using ZavaStorefront.Services;
using ZavaStorefront.Features;
using ZavaStorefront.Models;

namespace ZavaStorefront.Controllers
{
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly CartService _cartService;
        private readonly IFeatureFlagService _featureFlagService;
        private readonly IConfiguration _configuration;

        public CartController(ILogger<CartController> logger, CartService cartService, IFeatureFlagService featureFlagService, IConfiguration configuration)
        {
            _logger = logger;
            _cartService = cartService;
            _featureFlagService = featureFlagService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Loading cart page with {ItemCount} items", _cartService.GetCartItemCount());
            var cart = _cartService.GetCart();
            var total = _cartService.GetCartTotal();
            var discountedTotal = await _cartService.GetCartTotalWithDiscountAsync();
            var isBulkDiscountEnabled = await _featureFlagService.IsFeatureEnabledAsync(FeatureFlags.BulkDiscounts);

            var viewModel = new CartViewModel
            {
                Items = cart,
                ItemCount = cart.Sum(i => i.Quantity),
                Total = total,
                DiscountedTotal = discountedTotal,
                Savings = Math.Max(0, total - discountedTotal),
                IsBulkDiscountEnabled = isBulkDiscountEnabled,
                FirstTierThreshold = decimal.Parse(_configuration["BulkDiscounts:FirstTier:Threshold"] ?? "50"),
                FirstTierDiscount = int.Parse(_configuration["BulkDiscounts:FirstTier:Discount"] ?? "5"),
                SecondTierThreshold = decimal.Parse(_configuration["BulkDiscounts:SecondTier:Threshold"] ?? "100"),
                SecondTierDiscount = int.Parse(_configuration["BulkDiscounts:SecondTier:Discount"] ?? "10")
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            _logger.LogInformation("Updating product {ProductId} quantity to {Quantity}", productId, quantity);
            _cartService.UpdateQuantity(productId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            _logger.LogInformation("Removing product {ProductId} from cart", productId);
            _cartService.RemoveFromCart(productId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Checkout()
        {
            var itemCount = _cartService.GetCartItemCount();
            var total = _cartService.GetCartTotal();
            _logger.LogInformation("Processing checkout for {ItemCount} items, total: {Total:C}", itemCount, total);
            _logger.LogInformation("Telemetry: Checkout_Start itemCount={ItemCount} total={Total}", itemCount, total);
            _cartService.ClearCart();
            return RedirectToAction("CheckoutSuccess");
        }

        public IActionResult CheckoutSuccess()
        {
            _logger.LogInformation("Displaying checkout success page");
            return View();
        }

        public int GetCartCount()
        {
            return _cartService.GetCartItemCount();
        }
    }
}
