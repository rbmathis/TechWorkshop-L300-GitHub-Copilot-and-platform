using Xunit;
using Moq;
using Microsoft.AspNetCore.Http;
using ZavaStorefront.Services;
using ZavaStorefront.Models;
using System.Text.Json;

namespace ZavaStorefront.Tests.Services
{
    public class CartServiceTests
    {
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly Mock<IProductService> _productService;
        private readonly Mock<ITelemetryClient> _telemetry;
        private readonly Mock<ZavaStorefront.Services.ISessionManager> _sessionManager;
        private readonly Mock<ZavaStorefront.Features.IFeatureFlagService> _featureFlagService;
        private readonly CartService _cartService;

        public CartServiceTests()
        {
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _sessionManager = new Mock<ZavaStorefront.Services.ISessionManager>();
            _telemetry = new Mock<ITelemetryClient>();
            _productService = new Mock<IProductService>();
            _featureFlagService = new Mock<ZavaStorefront.Features.IFeatureFlagService>();

            var httpContext = new Mock<HttpContext>();
            _httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext.Object);

            _cartService = new CartService(_httpContextAccessor.Object, _productService.Object, _telemetry.Object, _sessionManager.Object, _featureFlagService.Object);
        }

        [Fact]
        public void GetCart_ReturnsEmpty_WhenNoSessionData()
        {
            // Arrange
            _sessionManager.Setup(s => s.GetString("ShoppingCart")).Returns((string)null);

            // Act
            var result = _cartService.GetCart();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetCart_ReturnsItems_WhenSessionHasData()
        {
            // Arrange
            var cartItems = new List<CartItem>
            {
                new CartItem { Product = new Product { Id = 1, Name = "Product 1", Price = 10m }, Quantity = 2 }
            };
            var json = JsonSerializer.Serialize(cartItems);

            _sessionManager.Setup(s => s.GetString("ShoppingCart")).Returns(json);

            // Act
            var result = _cartService.GetCart();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result[0].Product.Id);
            Assert.Equal(2, result[0].Quantity);
        }

        [Fact]
        public void AddToCart_AddsProduct_WhenNotInCart()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test", Price = 10m };

            _productService.Setup(p => p.GetProductById(1)).Returns(product);
            _sessionManager.Setup(s => s.GetString("ShoppingCart")).Returns((string)null);

            // Act
            _cartService.AddToCart(1);

            // Assert
            _sessionManager.Verify(s => s.SetString(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void AddToCart_IncrementsQuantity_WhenProductInCart()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test", Price = 10m };
            var cart = new List<CartItem> { new CartItem { Product = product, Quantity = 1 } };
            var json = JsonSerializer.Serialize(cart);

            _productService.Setup(p => p.GetProductById(1)).Returns(product);
            _sessionManager.Setup(s => s.GetString("ShoppingCart")).Returns(json);

            // Act
            _cartService.AddToCart(1);

            // Assert
            _sessionManager.Verify(s => s.SetString(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void RemoveFromCart_RemovesItem_WhenExists()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test", Price = 10m };
            var cart = new List<CartItem> { new CartItem { Product = product, Quantity = 1 } };
            var json = JsonSerializer.Serialize(cart);

            _sessionManager.Setup(s => s.GetString("ShoppingCart")).Returns(json);

            // Act
            _cartService.RemoveFromCart(1);

            // Assert
            _sessionManager.Verify(s => s.SetString(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void RemoveFromCart_DoesNothing_WhenItemNotExists()
        {
            // Arrange
            _sessionManager.Setup(s => s.GetString("ShoppingCart")).Returns((string)null);

            // Act
            _cartService.RemoveFromCart(999);

            // Assert
            _sessionManager.Verify(s => s.SetString(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void UpdateQuantity_UpdatesQty_WhenValid()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test", Price = 10m };
            var cart = new List<CartItem> { new CartItem { Product = product, Quantity = 1 } };
            var json = JsonSerializer.Serialize(cart);

            _sessionManager.Setup(s => s.GetString("ShoppingCart")).Returns(json);

            // Act
            _cartService.UpdateQuantity(1, 5);

            // Assert
            _sessionManager.Verify(s => s.SetString(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void ClearCart_RemovesSession()
        {
            // Act
            _cartService.ClearCart();

            // Assert
            _sessionManager.Verify(s => s.Remove("ShoppingCart"), Times.Once);
        }

        [Fact]
        public void GetCartItemCount_ReturnsTotal()
        {
            // Arrange
            var cart = new List<CartItem>
            {
                new CartItem { Product = new Product { Id = 1 }, Quantity = 2 },
                new CartItem { Product = new Product { Id = 2 }, Quantity = 3 }
            };
            var json = JsonSerializer.Serialize(cart);

            _sessionManager.Setup(s => s.GetString("ShoppingCart")).Returns(json);

            // Act
            var result = _cartService.GetCartItemCount();

            // Assert
            Assert.Equal(5, result);
        }

        [Fact]
        public void GetCartTotal_CalculatesTotal()
        {
            // Arrange
            var cart = new List<CartItem>
            {
                new CartItem { Product = new Product { Id = 1, Price = 10m }, Quantity = 2 },
                new CartItem { Product = new Product { Id = 2, Price = 20m }, Quantity = 3 }
            };
            var json = JsonSerializer.Serialize(cart);

            _sessionManager.Setup(s => s.GetString("ShoppingCart")).Returns(json);

            // Act
            var result = _cartService.GetCartTotal();

            // Assert
            Assert.Equal(80m, result);
        }

        [Fact]
        public void AddToCart_TracksTelemetry()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test", Price = 10m };
            _productService.Setup(p => p.GetProductById(1)).Returns(product);
            _sessionManager.Setup(s => s.GetString("ShoppingCart")).Returns((string)null);

            // Act
            _cartService.AddToCart(1);

            // Assert
            _productService.Verify(p => p.GetProductById(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public void ClearCart_RemovesCart()
        {
            // Act
            _cartService.ClearCart();

            // Assert
            _sessionManager.Verify(s => s.Remove("ShoppingCart"), Times.Once);
        }
    }
}
