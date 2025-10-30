using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Services;

// TODO: Implement methods
public class FoodTruckService : IFoodTruckService
{
    private readonly IDtuFoodDbContext _dbContext;
    private readonly IGuidGenerator _guidGenerator;

    public FoodTruckService(IDtuFoodDbContext dbContext,
        IGuidGenerator guidGenerator)
    {
        _dbContext = dbContext;
        _guidGenerator = guidGenerator;
    }

    public async Task<FoodTruck> CreateFoodTruck(FoodTruckRegistry foodTruckRegistry, CancellationToken cancellationToken = default)
    {

        var newTruck = new FoodTruck()
        {
            Id = _guidGenerator.NewGuid(),
            Name = foodTruckRegistry.Name,
            GpsLatitude = foodTruckRegistry.GpsLatitude,
            GpsLongitude = foodTruckRegistry.GpsLongitude,
            Products = new List<Product>(),
            Availability = new List<Availability>(),
            Managers = new List<User>()
        };
        
        var result = await _dbContext.FoodTrucks.AddAsync(newTruck, cancellationToken: cancellationToken);
        
        await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return result.Entity;
    }

    public async Task<List<FoodTruck>> GetAllFoodTrucks(CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodTrucks.ToListAsync(cancellationToken);
    }

    public async Task<FoodTruck?> GetFoodTruckById(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodTrucks.FindAsync([id], cancellationToken: cancellationToken);
    }

    public async Task<FoodTruck?> UpdateFoodTruck(Guid id, FoodTruckRegistry foodTruckRegistry, CancellationToken cancellationToken = default)
    {
        var foodTruck = await _dbContext.FoodTrucks.FindAsync([id], cancellationToken: cancellationToken);
        if (foodTruck is null) return null;
        
        foodTruck.Name        = foodTruckRegistry.Name;
        foodTruck.GpsLatitude = foodTruckRegistry.GpsLatitude;
        foodTruck.GpsLongitude= foodTruckRegistry.GpsLongitude;

        await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);
        return foodTruck;
    }


    public async Task<bool> DeleteFoodTruck(Guid id, CancellationToken cancellationToken = default)
    {
        var foodTruck = await _dbContext.FoodTrucks.FindAsync([id], cancellationToken: cancellationToken);
        if (foodTruck is null) return false;

        _dbContext.FoodTrucks.Remove(foodTruck);
        await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);
        return true;
    }

    public async Task<bool> FoodTruckExists(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodTrucks.AnyAsync(x => x.Id == id, cancellationToken);
    }
}

public interface IFoodTruckService
{
    /// <summary>
    /// Creates a new food truck in the database from the food truck registry parameter
    /// </summary>
    /// <param name="foodTruckRegistry">The food truck registry</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The created food truck entity</returns>
    Task<FoodTruck> CreateFoodTruck(FoodTruckRegistry foodTruckRegistry, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all food trucks
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>All food trucks</returns>
    Task<List<FoodTruck>> GetAllFoodTrucks(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds a food truck from a specific id
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The food truck, or null if the food truck doesn't exist</returns>
    Task<FoodTruck?> GetFoodTruckById(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the food truck with a specific id with the data in the food truck registry
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="foodTruckRegistry">The new data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated food truck, or null if the food truck doesn't exist</returns>
    /// <remarks>This doesn't update the products in the food truck</remarks>
    Task<FoodTruck?> UpdateFoodTruck(Guid id,
        FoodTruckRegistry foodTruckRegistry,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a food truck with a specific id
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the food truck was deleted, false if the food truck was not found</returns>
    Task<bool> DeleteFoodTruck(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a food truck with a specific id exists
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the food truck exists, otherwise false</returns>
    Task<bool> FoodTruckExists(Guid id, CancellationToken cancellationToken = default);
}