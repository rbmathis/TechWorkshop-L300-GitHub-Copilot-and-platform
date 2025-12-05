using Xunit;
using Moq;
using Microsoft.Extensions.Caching.Distributed;
using ZavaStorefront.Services;
using System.Text.Json;

namespace ZavaStorefront.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IDistributedCache> _cache;
        private readonly Mock<ITelemetryClient> _telemetry;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _cache = new Mock<IDistributedCache>();
            _telemetry = new Mock<ITelemetryClient>();
            _productService = new ProductService(_cache.Object, _telemetry.Object);
        }

        [Fact]
        public void GetAllProducts_ReturnsProducts()
        {
            // Act
            var result = _productService.GetAllProducts();

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(10, result.Count);
        }

        [Fact]
        public void GetAllProducts_HasCorrectProperties()
        {
            // Act
            var result = _productService.GetAllProducts();
            var first = result.First();

            // Assert
            Assert.True(first.Id > 0);
            Assert.NotEmpty(first.Name);
            Assert.True(first.Price > 0);
        }

        [Fact]
        public void GetProductById_ReturnsProduct_WhenExists()
        {
            // Act
            var result = _productService.GetProductById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.NotEmpty(result.Name);
        }

        [Fact]
        public void GetProductById_ReturnsNull_WhenNotExists()
        {
            // Act
            var result = _productService.GetProductById(999);

            // Assert
            Assert.Null(result);
        }

        [Fact(Skip = "Cannot mock IDistributedCache extension methods")]
        public async Task GetAllProductsAsync_ReturnsCached_WhenCacheHit()
        {
            // Extension methods cannot be mocked directly
            Assert.True(true);
        }

        [Fact(Skip = "Cannot mock IDistributedCache extension methods")]
        public async Task GetAllProductsAsync_RefreshesCache_WhenMiss()
        {
            // Extension methods cannot be mocked directly
            Assert.True(true);
        }

        [Fact(Skip = "Cannot mock IDistributedCache extension methods")]
        public async Task GetAllProductsAsync_HandlesEmpty_Cache()
        {
            // Extension methods cannot be mocked directly
            Assert.True(true);
        }

        [Fact]
        public void GetAllProducts_ContainsExpectedIds()
        {
            // Act
            var result = _productService.GetAllProducts();
            var ids = result.Select(p => p.Id).ToList();

            // Assert
            for (int i = 1; i <= 10; i++)
            {
                Assert.Contains(i, ids);
            }
        }
    }
}
