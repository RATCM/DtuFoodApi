using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Services;

public class ProductService : IProductService
{
    private readonly IDtuFoodDbContext _dbContext;
    private readonly IGuidGenerator _guidGenerator;
    public ProductService(IDtuFoodDbContext dbContext, IGuidGenerator guidGenerator)
    {
        _dbContext = dbContext;
        _guidGenerator = guidGenerator;
    }

    public async Task<ProductDto?> CreateProduct(Guid foodTruckId, ProductRegistry productRegistry, CancellationToken cancellationToken = default)
    {
        //get foodtruck matching the id
        var foodTruck = await _dbContext.FoodTrucks
            .FirstOrDefaultAsync(ft => ft.Id == foodTruckId, cancellationToken);

        if (foodTruck is null) return null;
        
        var newProduct = new Product()
        {
            FoodTruck = foodTruck,
            Name = productRegistry.Name,
            Price = decimal.Parse(productRegistry.Price),
            Description = productRegistry.Description,
            Category = productRegistry.Category,
            Image = null
        };
        
        var result = await _dbContext.Products.AddAsync(newProduct, cancellationToken: cancellationToken);
        
        await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return result.Entity.ToDto();
    }

    public async Task<List<ProductDto>> GetAllProducts(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products.Select(x => x.ToDto()).ToListAsync(cancellationToken);
    }

    public async Task<List<ProductDto>> GetAllProductsFromFoodTruck(Guid foodTruckId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .Where(f => f.FoodTruck.Id == foodTruckId)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductDto?> GetProductByTruckIdAndProductName(Guid foodTruckId, string name, CancellationToken cancellationToken = default)
    {
        //New object cuz composite key to find unique product
        return await _dbContext.Products
            .FindAsync([foodTruckId, name], cancellationToken: cancellationToken).ToDto();
    }

    public async Task<Image?> GetProductImage(Guid foodTruckId, string name, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .Include(x => x.Image)
            .FirstOrDefaultAsync(x => x.FoodTruck.Id == foodTruckId && x.Name == name, cancellationToken);
        return product?.Image;
    }

    public async Task<Image?> UpdateProductImage(Guid foodTruckId, 
        string name, 
        byte[] image, 
        CancellationToken cancellationToken = default)
    {
        var product = await _dbContext.Products
            .Include(x => x.Image)
            .FirstOrDefaultAsync(x =>
                    x.FoodTruck.Id == foodTruckId &&
                    x.Name == name,
                cancellationToken);

        if (product is null) return null;

        if (product.Image is not null)
            _dbContext.Images.Remove(product.Image);

        var imageEntry = _dbContext.Images.Add(new Image
        {
            Id = _guidGenerator.NewGuid(),
            Blob = image
        });

        product.Image = imageEntry.Entity;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return product.Image;
    }

    public async Task<ProductDto?> UpdateProduct(Guid foodTruckId, string name, ProductRegistry productRegistry,
        CancellationToken cancellationToken = default)
    {
        // Find the product by its composite key (FoodTruckId + Name)
        var product = await _dbContext.Products.FindAsync(
            [foodTruckId, name], cancellationToken: cancellationToken);

        if (product is null)
            return null;

        // Update
        product.Price = decimal.Parse(productRegistry.Price);
        product.Description = productRegistry.Description;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return product.ToDto();
    }

    public async Task<bool> DeleteProduct(Guid foodTruckId, string name, CancellationToken cancellationToken = default)
    {
        // Find the product by its composite key (FoodTruckId + Name)
        var product = await _dbContext.Products.FindAsync(
            [foodTruckId, name], cancellationToken: cancellationToken);

        if (product is null)
            return false;
        
        _dbContext.Products.Remove(product);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ProductExists(Guid foodTruckId, string name, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products.AnyAsync
            (x => x.FoodTruck.Id == foodTruckId && x.Name == name, cancellationToken);
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
    Task<ProductDto?> CreateProduct(Guid foodTruckId,
        ProductRegistry productRegistry,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all products
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>All products</returns>
    Task<List<ProductDto>> GetAllProducts(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all products from a specific food truck
    /// </summary>
    /// <param name="foodTruckId">The food truck id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>All products from the food truck</returns>
    Task<List<ProductDto>> GetAllProductsFromFoodTruck(Guid foodTruckId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds a product from a specific food truck and name
    /// </summary>
    /// <param name="foodTruckId">The food truck id</param>
    /// <param name="name">The product name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The product, or null if the product doesn't exist</returns>
    /// <remarks>Product names are case-insensitive</remarks>
    Task<ProductDto?> GetProductByTruckIdAndProductName(Guid foodTruckId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the image from a product
    /// </summary>
    /// <param name="foodTruckId">The food truck id</param>
    /// <param name="name">The product name</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The product image</returns>
    Task<Image?> GetProductImage(Guid foodTruckId, string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the product image
    /// </summary>
    /// <param name="foodTruckId">The food truck id</param>
    /// <param name="name">The product name</param>
    /// <param name="image">The raw image in bytes</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated image</returns>
    Task<Image?> UpdateProductImage(Guid foodTruckId,
        string name, 
        byte[] image,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates the product with a specific id with the data in the product registry
    /// </summary>
    /// <param name="foodTruckId">The food truck id</param>
    /// <param name="name">The product name</param>
    /// <param name="productRegistry">The new data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated product, or null if the product doesn't exist</returns>
    Task<ProductDto?> UpdateProduct(Guid foodTruckId,
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