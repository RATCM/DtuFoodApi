using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Services;

// TODO: Implement methods
public class ProductService : IProductService
{
    private readonly IDtuFoodDbContext _dbContext;
    
    public ProductService(IDtuFoodDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product> CreateProduct(Guid foodTruckId, ProductRegistry productRegistry, CancellationToken cancellationToken = default)
    {
        //get foodtruck matching the id
        var foodTruck = await _dbContext.FoodTrucks
            .FirstOrDefaultAsync(ft => ft.Id == foodTruckId, cancellationToken);
        
        var newProduct = new Product()
        {
            FoodTruck = foodTruck,
            Name = productRegistry.Name,
            Price = productRegistry.Price,
            Description = productRegistry.Description,
        };
        
        var result = await _dbContext.Products.AddAsync(newProduct, cancellationToken: cancellationToken);
        
        await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return result.Entity;
    }

    public async Task<List<Product>> GetAllProducts(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products.ToListAsync(cancellationToken);
    }

    public async Task<List<Product>> GetAllProductsFromFoodTruck(Guid foodTruckId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products.Where(f => f.FoodTruck.Id == foodTruckId).ToListAsync(cancellationToken);
        
    }

    public async Task<Product?> GetProductByTruckIdAndProductName(Guid foodTruckId, string name, CancellationToken cancellationToken = default)
    {
        //New object cuz composite key to find unique product
        return await _dbContext.Products.FindAsync(new object[] { foodTruckId, name }, cancellationToken: cancellationToken);
    }

    public async Task<Product?> UpdateProduct(Guid foodTruckId, string name, ProductRegistry productRegistry,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteProduct(Guid foodTruckId, string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ProductExists(Guid foodTruckId, string name, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

public interface IProductService
{
    /// <summary>
    /// Creates a new product for a food truck in the database
    /// </summary>
    /// <param name="foodTruckId">The food truck id</param>
    /// <param name="productRegistry">The product registry</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The created product entity</returns>
    /// <remarks>Products in the same food truck cannot have the same name</remarks>
    Task<Product> CreateProduct(Guid foodTruckId,
        ProductRegistry productRegistry,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all products
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>All products</returns>
    Task<List<Product>> GetAllProducts(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all products from a specific food truck
    /// </summary>
    /// <param name="foodTruckId">The food truck id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>All products from the food truck</returns>
    Task<List<Product>> GetAllProductsFromFoodTruck(Guid foodTruckId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds a product from a specific food truck and name
    /// </summary>
    /// <param name="foodTruckId">The food truck id</param>
    /// <param name="name">The product name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The product, or null if the product doesn't exist</returns>
    /// <remarks>Product names are case-insensitive</remarks>
    Task<Product?> GetProductByTruckIdAndProductName(Guid foodTruckId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the product with a specific id with the data in the product registry
    /// </summary>
    /// <param name="foodTruckId">The food truck id</param>
    /// <param name="name">The product name</param>
    /// <param name="productRegistry">The new data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated product, or null if the product doesn't exist</returns>
    Task<Product?> UpdateProduct(Guid foodTruckId,
        string name,
        ProductRegistry productRegistry,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a product with a specific id
    /// </summary>
    /// <param name="foodTruckId">The food truck id</param>
    /// <param name="name">The product name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the product was deleted, false if the product was not found</returns>
    Task<bool> DeleteProduct(Guid foodTruckId,
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a product with a specific id exists
    /// </summary>
    /// <param name="foodTruckId">The food truck id</param>
    /// <param name="name">The product name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the product exists, otherwise false</returns>
    Task<bool> ProductExists(Guid foodTruckId,
        string name,
        CancellationToken cancellationToken = default);
}