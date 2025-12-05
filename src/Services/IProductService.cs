using ZavaStorefront.Models;

namespace ZavaStorefront.Services
{
    /// <summary>
    /// Interface for product service to enable mocking in tests.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Get all products.
        /// </summary>
        List<Product> GetAllProducts();

        /// <summary>
        /// Get all products asynchronously.
        /// </summary>
        Task<List<Product>> GetAllProductsAsync();

        /// <summary>
        /// Get product by ID.
        /// </summary>
        Product? GetProductById(int id);
    }
}
