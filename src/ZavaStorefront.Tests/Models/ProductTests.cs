using Xunit;
using ZavaStorefront.Models;

namespace ZavaStorefront.Tests.Models
{
    public class ProductTests
    {
        [Fact]
        public void Product_CanBeCreated()
        {
            // Act
            var product = new Product { Id = 1, Name = "Test", Price = 10.00m };

            // Assert
            Assert.NotNull(product);
            Assert.Equal(1, product.Id);
            Assert.Equal("Test", product.Name);
            Assert.Equal(10.00m, product.Price);
        }

        [Fact]
        public void Product_PropertiesCanBeSet()
        {
            // Arrange
            var product = new Product();

            // Act
            product.Id = 5;
            product.Name = "Updated";
            product.Price = 25.50m;

            // Assert
            Assert.Equal(5, product.Id);
            Assert.Equal("Updated", product.Name);
            Assert.Equal(25.50m, product.Price);
        }

        [Fact]
        public void Product_DefaultValuesAreNull()
        {
            // Act
            var product = new Product();

            // Assert
            Assert.Equal(0, product.Id);
            Assert.Null(product.Name);
            Assert.Equal(0, product.Price);
        }
    }

    public class CartItemTests
    {
        [Fact]
        public void CartItem_CanBeCreated()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Test", Price = 10m };

            // Act
            var item = new CartItem { Product = product, Quantity = 2 };

            // Assert
            Assert.NotNull(item);
            Assert.Equal(product, item.Product);
            Assert.Equal(2, item.Quantity);
        }

        [Fact]
        public void CartItem_PropertiesCanBeModified()
        {
            // Arrange
            var item = new CartItem();

            // Act
            item.Product = new Product { Id = 1 };
            item.Quantity = 5;

            // Assert
            Assert.NotNull(item.Product);
            Assert.Equal(5, item.Quantity);
        }
    }

    public class ErrorViewModelTests
    {
        [Fact]
        public void ErrorViewModel_CanBeCreated()
        {
            // Act
            var vm = new ErrorViewModel { RequestId = "12345" };

            // Assert
            Assert.Equal("12345", vm.RequestId);
        }

        [Fact]
        public void ErrorViewModel_ShowRequestId_IsTrueWhenSet()
        {
            // Arrange
            var vm = new ErrorViewModel { RequestId = "123" };

            // Act
            var result = vm.ShowRequestId;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ErrorViewModel_ShowRequestId_IsFalseWhenNull()
        {
            // Arrange
            var vm = new ErrorViewModel { RequestId = null };

            // Act
            var result = vm.ShowRequestId;

            // Assert
            Assert.False(result);
        }
    }
}
