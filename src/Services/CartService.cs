using ZavaStorefront.Models;
using ZavaStorefront.Features;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ZavaStorefront.Services
{
    public class CartService
    {
        private const string CartSessionKey = "ShoppingCart";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;
        private readonly ITelemetryClient _telemetry;
        private readonly ISessionManager _sessionManager;
        private readonly IFeatureFlagService _featureFlagService;

        public CartService(IHttpContextAccessor httpContextAccessor, IProductService productService, ITelemetryClient telemetry, ISessionManager sessionManager, IFeatureFlagService featureFlagService)
        {
            _httpContextAccessor = httpContextAccessor;
            _productService = productService;
            _telemetry = telemetry;
            _sessionManager = sessionManager;
            _featureFlagService = featureFlagService;
        }

        public List<CartItem> GetCart()
        {
            var cartJson = _sessionManager.GetString(CartSessionKey);

            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }

            return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        public void AddToCart(int productId)
        {
            var cart = GetCart();
            var product = _productService.GetProductById(productId);
            var userContext = GetUserContext();

            if (product == null)
            {
                _telemetry.TrackEvent("Cart_Add_InvalidProduct", MergeContext(userContext, new Dictionary<string, string>
                {
                    { "productId", productId.ToString() }
                }));
                return;
            }

            var existingItem = cart.FirstOrDefault(item => item.Product?.Id == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    Product = product,
                    Quantity = 1
                });
            }

            SaveCart(cart);

            _telemetry.TrackEvent("Cart_Add", MergeContext(userContext, new Dictionary<string, string>
            {
                { "productId", product.Id.ToString() },
                { "productName", product.Name ?? "Unknown" },
                { "unitPrice", product.Price.ToString("F2") },
                { "quantity", cart.First(i => i.Product?.Id == productId).Quantity.ToString() }
            }));
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.Product?.Id == productId);
            var userContext = GetUserContext();

            if (item?.Product != null)
            {
                cart.Remove(item);
                SaveCart(cart);

                _telemetry.TrackEvent("Cart_Remove", MergeContext(userContext, new Dictionary<string, string>
                {
                    { "productId", productId.ToString() },
                    { "productName", item.Product.Name ?? "Unknown" }
                }));
            }
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.Product?.Id == productId);
            var userContext = GetUserContext();

            if (item?.Product != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                    _telemetry.TrackEvent("Cart_Update_Remove", MergeContext(userContext, new Dictionary<string, string>
                    {
                        { "productId", productId.ToString() },
                        { "productName", item.Product.Name ?? "Unknown" }
                    }));
                }
                else
                {
                    item.Quantity = quantity;
                    _telemetry.TrackEvent("Cart_UpdateQuantity", MergeContext(userContext, new Dictionary<string, string>
                    {
                        { "productId", productId.ToString() },
                        { "productName", item.Product.Name ?? "Unknown" },
                        { "quantity", quantity.ToString() }
                    }));
                }
                SaveCart(cart);
            }
        }

        public void ClearCart()
        {
            _sessionManager.Remove(CartSessionKey);
            _telemetry.TrackEvent("Cart_Clear", MergeContext(GetUserContext(), null));
        }

        private Dictionary<string, string> GetUserContext()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var userId = httpContext?.User?.Identity?.IsAuthenticated == true
                ? httpContext.User.Identity.Name
                : "anonymous";
            var sessionId = httpContext?.Session?.Id ?? "no-session";

            return new Dictionary<string, string>
            {
                { "userId", userId ?? "anonymous" },
                { "sessionId", sessionId }
            };
        }

        private Dictionary<string, string> MergeContext(Dictionary<string, string> userContext, Dictionary<string, string>? properties)
        {
            var merged = new Dictionary<string, string>(userContext);
            if (properties != null)
            {
                foreach (var kvp in properties)
                {
                    merged[kvp.Key] = kvp.Value;
                }
            }
            return merged;
        }

        public int GetCartItemCount()
        {
            var cart = GetCart();
            return cart.Sum(item => item.Quantity);
        }

        public decimal GetCartTotal()
        {
            var cart = GetCart();
            return cart.Sum(item => (item.Product?.Price ?? 0) * item.Quantity);
        }

        /// <summary>
        /// Get cart total with potential bulk discount if feature is enabled
        /// </summary>
        public async Task<decimal> GetCartTotalWithDiscountAsync()
        {
            var baseTotal = GetCartTotal();

            // Apply bulk discount if feature is enabled and conditions are met
            return await _featureFlagService.ExecuteIfEnabledAsync(
                FeatureFlags.BulkDiscounts,
                async () =>
                {
                    var discountedTotal = ApplyBulkDiscount(baseTotal);
                    _telemetry.TrackEvent("BulkDiscount_Applied", new Dictionary<string, string>
                    {
                        { "originalTotal", baseTotal.ToString("F2") },
                        { "discountedTotal", discountedTotal.ToString("F2") }
                    });
                    return await Task.FromResult(discountedTotal);
                },
                baseTotal);
        }

        private decimal ApplyBulkDiscount(decimal total)
        {
            // Apply 10% discount for orders over $100
            if (total > 100m)
            {
                return total * 0.9m;
            }
            // Apply 5% discount for orders over $50
            else if (total > 50m)
            {
                return total * 0.95m;
            }
            return total;
        }

        private void SaveCart(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            _sessionManager.SetString(CartSessionKey, cartJson);
        }
    }
}
