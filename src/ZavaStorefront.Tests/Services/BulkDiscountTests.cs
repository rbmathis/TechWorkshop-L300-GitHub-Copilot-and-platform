using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using ZavaStorefront.Services;
using ZavaStorefront.Models;
using ZavaStorefront.Features;
using System.Text.Json;

namespace ZavaStorefront.Tests.Services
{
    public class BulkDiscountTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IProductService> _productService;
        private readonly Mock<ITelemetryClient> _telemetry;
        private readonly Mock<ISessionManager> _sessionManager;
        private readonly Mock<IFeatureFlagService> _featureFlagService;

        public BulkDiscountTests()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _sessionManager = new Mock<ISessionManager>();
            _telemetry = new Mock<ITelemetryClient>();
            _productService = new Mock<IProductService>();
            _featureFlagService = new Mock<IFeatureFlagService>();

            var httpContext = new Mock<HttpContext>();
            _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext.Object);
        }

        private CartService CreateCartService(BulkDiscountOptions options)
        {
            var optionsMock = new Mock<IOptions<BulkDiscountOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            return new CartService(
                _httpContextAccessor.Object,
                _productService.Object,
                _telemetry.Object,
                _sessionManager.Object,
                _featureFlagService.Object,
                optionsMock.Object
            );
        }

        private void SetupCart(CartService service, List<CartItem> items)
        {
            var json = JsonSerializer.Serialize(items);
            _sessionManager.Setup(s => s.GetString("ShoppingCart")).Returns(json);
        }

        private void SetupFeatureFlagEnabled()
        {
            _featureFlagService
                .Setup(f => f.ExecuteIfEnabledAsync(
                    FeatureFlags.BulkDiscounts,
                    It.IsAny<Func<Task<decimal>>>(),
                    It.IsAny<decimal>()))
                .Returns<string, Func<Task<decimal>>, decimal>(async (flag, action, defaultValue) => await action());
        }

        private void SetupFeatureFlagDisabled()
        {
            _featureFlagService
                .Setup(f => f.ExecuteIfEnabledAsync(
                    FeatureFlags.BulkDiscounts,
                    It.IsAny<Func<Task<decimal>>>(),
                    It.IsAny<decimal>()))
                .Returns<string, Func<Task<decimal>>, decimal>((flag, action, defaultValue) => Task.FromResult(defaultValue));
        }

        [Fact]
        public async Task GetCartTotalWithDiscountAsync_NoDiscount_WhenFeatureDisabled()
        {
            // Arrange
            var options = new BulkDiscountOptions
            {
                ThresholdLow = 50m,
                RateLow = 0.05m,
                ThresholdHigh = 100m,
                RateHigh = 0.10m
            };
            var cartService = CreateCartService(options);

            var cart = new List<CartItem>
            {
                new CartItem { Product = new Product { Id = 1, Price = 120m }, Quantity = 1 }
            };
            SetupCart(cartService, cart);
            SetupFeatureFlagDisabled();

            // Act
            var result = await cartService.GetCartTotalWithDiscountAsync();

            // Assert
            Assert.Equal(120m, result);
        }

        [Fact]
        public async Task GetCartTotalWithDiscountAsync_NoDiscount_WhenBelowLowThreshold()
        {
            // Arrange
            var options = new BulkDiscountOptions
            {
                ThresholdLow = 50m,
                RateLow = 0.05m,
                ThresholdHigh = 100m,
                RateHigh = 0.10m
            };
            var cartService = CreateCartService(options);

            var cart = new List<CartItem>
            {
                new CartItem { Product = new Product { Id = 1, Price = 40m }, Quantity = 1 }
            };
            SetupCart(cartService, cart);
            SetupFeatureFlagEnabled();

            // Act
            var result = await cartService.GetCartTotalWithDiscountAsync();

            // Assert
            Assert.Equal(40m, result);
        }

        [Fact]
        public async Task GetCartTotalWithDiscountAsync_LowDiscount_WhenAtLowThreshold()
        {
            // Arrange
            var options = new BulkDiscountOptions
            {
                ThresholdLow = 50m,
                RateLow = 0.05m,
                ThresholdHigh = 100m,
                RateHigh = 0.10m
            };
            var cartService = CreateCartService(options);

            var cart = new List<CartItem>
            {
                new CartItem { Product = new Product { Id = 1, Price = 50m }, Quantity = 1 }
            };
            SetupCart(cartService, cart);
            SetupFeatureFlagEnabled();

            // Act
            var result = await cartService.GetCartTotalWithDiscountAsync();

            // Assert
            Assert.Equal(47.5m, result); // 50 * 0.95 = 47.5
        }

        [Fact]
        public async Task GetCartTotalWithDiscountAsync_LowDiscount_WhenBetweenThresholds()
        {
            // Arrange
            var options = new BulkDiscountOptions
            {
                ThresholdLow = 50m,
                RateLow = 0.05m,
                ThresholdHigh = 100m,
                RateHigh = 0.10m
            };
            var cartService = CreateCartService(options);

            var cart = new List<CartItem>
            {
                new CartItem { Product = new Product { Id = 1, Price = 75m }, Quantity = 1 }
            };
            SetupCart(cartService, cart);
            SetupFeatureFlagEnabled();

            // Act
            var result = await cartService.GetCartTotalWithDiscountAsync();

            // Assert
            Assert.Equal(71.25m, result); // 75 * 0.95 = 71.25
        }

        [Fact]
        public async Task GetCartTotalWithDiscountAsync_HighDiscount_WhenAtHighThreshold()
        {
            // Arrange
            var options = new BulkDiscountOptions
            {
                ThresholdLow = 50m,
                RateLow = 0.05m,
                ThresholdHigh = 100m,
                RateHigh = 0.10m
            };
            var cartService = CreateCartService(options);

            var cart = new List<CartItem>
            {
                new CartItem { Product = new Product { Id = 1, Price = 100m }, Quantity = 1 }
            };
            SetupCart(cartService, cart);
            SetupFeatureFlagEnabled();

            // Act
            var result = await cartService.GetCartTotalWithDiscountAsync();

            // Assert
            Assert.Equal(90m, result); // 100 * 0.90 = 90
        }

        [Fact]
        public async Task GetCartTotalWithDiscountAsync_HighDiscount_WhenAboveHighThreshold()
        {
            // Arrange
            var options = new BulkDiscountOptions
            {
                ThresholdLow = 50m,
                RateLow = 0.05m,
                ThresholdHigh = 100m,
                RateHigh = 0.10m
            };
            var cartService = CreateCartService(options);

            var cart = new List<CartItem>
            {
                new CartItem { Product = new Product { Id = 1, Price = 150m }, Quantity = 1 }
            };
            SetupCart(cartService, cart);
            SetupFeatureFlagEnabled();

            // Act
            var result = await cartService.GetCartTotalWithDiscountAsync();

            // Assert
            Assert.Equal(135m, result); // 150 * 0.90 = 135
        }

        [Fact]
        public async Task GetCartTotalWithDiscountAsync_CustomThresholds_WorkCorrectly()
        {
            // Arrange
            var options = new BulkDiscountOptions
            {
                ThresholdLow = 200m,
                RateLow = 0.15m,
                ThresholdHigh = 500m,
                RateHigh = 0.25m
            };
            var cartService = CreateCartService(options);

            var cart = new List<CartItem>
            {
                new CartItem { Product = new Product { Id = 1, Price = 600m }, Quantity = 1 }
            };
            SetupCart(cartService, cart);
            SetupFeatureFlagEnabled();

            // Act
            var result = await cartService.GetCartTotalWithDiscountAsync();

            // Assert
            Assert.Equal(450m, result); // 600 * 0.75 = 450
        }

        [Fact]
        public async Task GetCartTotalWithDiscountAsync_MultipleItems_AppliesDiscountCorrectly()
        {
            // Arrange
            var options = new BulkDiscountOptions
            {
                ThresholdLow = 50m,
                RateLow = 0.05m,
                ThresholdHigh = 100m,
                RateHigh = 0.10m
            };
            var cartService = CreateCartService(options);

            var cart = new List<CartItem>
            {
                new CartItem { Product = new Product { Id = 1, Price = 30m }, Quantity = 2 },
                new CartItem { Product = new Product { Id = 2, Price = 25m }, Quantity = 2 }
            };
            SetupCart(cartService, cart);
            SetupFeatureFlagEnabled();

            // Act
            var result = await cartService.GetCartTotalWithDiscountAsync();

            // Assert - Total is 110, should get 10% discount
            Assert.Equal(99m, result); // 110 * 0.90 = 99
        }

        [Fact]
        public async Task GetCartTotalWithDiscountAsync_EdgeCase_JustBelowHighThreshold()
        {
            // Arrange
            var options = new BulkDiscountOptions
            {
                ThresholdLow = 50m,
                RateLow = 0.05m,
                ThresholdHigh = 100m,
                RateHigh = 0.10m
            };
            var cartService = CreateCartService(options);

            var cart = new List<CartItem>
            {
                new CartItem { Product = new Product { Id = 1, Price = 99.99m }, Quantity = 1 }
            };
            SetupCart(cartService, cart);
            SetupFeatureFlagEnabled();

            // Act
            var result = await cartService.GetCartTotalWithDiscountAsync();

            // Assert - Should get low tier discount (5%)
            Assert.Equal(94.99m, result, 2); // 99.99 * 0.95 ≈ 94.99
        }
    }
}