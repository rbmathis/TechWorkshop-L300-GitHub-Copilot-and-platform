using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZavaStorefront.Controllers;
using ZavaStorefront.Services;
using ZavaStorefront.Models;

namespace ZavaStorefront.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _logger;
        private readonly Mock<IProductService> _productService;
        private readonly Mock<CartService> _cartService;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _logger = new Mock<ILogger<HomeController>>();
            _productService = new Mock<IProductService>();
            _cartService = new Mock<CartService>(
                new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>().Object,
                _productService.Object,
                new Mock<ITelemetryClient>().Object,
                new Mock<ISessionManager>().Object);

            _controller = new HomeController(_logger.Object, _productService.Object, _cartService.Object);
        }

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Test", Price = 10m }
            };
            _productService.Setup(p => p.GetAllProductsAsync()).ReturnsAsync(products);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }
    }
}
