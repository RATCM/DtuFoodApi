using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;

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

        var newTruck = new FoodTruck
        {
            Id = _guidGenerator.NewGuid(),
            Name = foodTruckRegistry.Name,
            GPSLatitude = foodTruckRegistry.GpsLatitude,
            GPSLongitude = foodTruckRegistry.GpsLongitude,
        };

        _dbContext.FoodTrucks.Add(newTruck);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newTruck;
    }

    public async Task<List<FoodTruck>> GetAllFoodTrucks(CancellationToken cancellationToken = default)
    {

        //throw new NotImplementedException();
    }

    public async Task<FoodTruck?> GetFoodTruckById(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<FoodTruck?> UpdateFoodTruck(Guid id, FoodTruckRegistry foodTruckRegistry, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteFoodTruck(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> FoodTruckExists(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
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