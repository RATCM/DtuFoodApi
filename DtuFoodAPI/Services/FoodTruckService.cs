using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Extensions;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Image = DtuFoodAPI.Models.Image;

namespace DtuFoodAPI.Services;

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

    public async Task<FoodTruckDto> CreateFoodTruck(FoodTruckRegistry foodTruckRegistry, CancellationToken cancellationToken = default)
    {

        var newTruck = new FoodTruck()
        {
            Id = _guidGenerator.NewGuid(),
            Name = foodTruckRegistry.Name,
            GpsLatitude = foodTruckRegistry.GpsLatitude,
            GpsLongitude = foodTruckRegistry.GpsLongitude,
            Products = new List<Product>(),
            Availability = new List<Availability>(),
            Managers = new List<User>(),
            PageBanner = null,
        };
        
        var result = await _dbContext.FoodTrucks.AddAsync(newTruck, cancellationToken: cancellationToken);
        
        await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

        return result.Entity.ToDto();
    }

    public async Task<List<FoodTruckDto>> GetAllFoodTrucks(CancellationToken cancellationToken = default)
    {
        return await _dbContext.FoodTrucks
            .Include(x => x.Products)
            .Include(x => x.Availability)
            .Select(x => x.ToDto()).ToListAsync(cancellationToken);
    }

    public async Task<FoodTruckDto?> GetFoodTruckById(Guid id, CancellationToken cancellationToken = default)
    {
        var truck = await _dbContext.FoodTrucks
            .Include(x => x.Products)
            .Include(x => x.Availability)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return truck?.ToDto();
    }

    public async Task<Image?> GetFoodTruckHomeBanner(Guid id, CancellationToken cancellationToken = default)
    {
        var truck = await _dbContext.FoodTrucks
            .Include(x => x.HomeBanner)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return truck?.HomeBanner;
    }
    
    public async Task<Image?> GetFoodTruckPageBanner(Guid id, CancellationToken cancellationToken = default)
    {
        var truck = await _dbContext.FoodTrucks
            .Include(x => x.PageBanner)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return truck?.PageBanner;
    }

    public async Task<List<UserDto>> GetManagers(Guid id, CancellationToken cancellationToken = default)
    {
        var truck = await _dbContext.FoodTrucks
            .Include(x => x.Managers)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return truck?.Managers.Select(x => x.ToDto()).ToList() ?? [];
    }
    
    public async Task<FoodTruckDto?> UpdateFoodTruck(Guid id, FoodTruckRegistry foodTruckRegistry, CancellationToken cancellationToken = default)
    {
        var foodTruck = await _dbContext.FoodTrucks.FindAsync([id], cancellationToken: cancellationToken);
        if (foodTruck is null) return null;
        
        foodTruck.Name        = foodTruckRegistry.Name;
        foodTruck.GpsLatitude = foodTruckRegistry.GpsLatitude;
        foodTruck.GpsLongitude= foodTruckRegistry.GpsLongitude;

        await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);
        return foodTruck.ToDto();
    }

    public async Task<Image?> UpdateFoodTruckHomeBanner(Guid id,
        byte[] image,
        CancellationToken cancellationToken = default)
    {
        var foodTruck = await _dbContext.FoodTrucks
            .Include(x => x.HomeBanner)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (foodTruck is null) return null;

        // Remove old image
        if (foodTruck.HomeBanner is not null)
            _dbContext.Images.Remove(foodTruck.HomeBanner);

        using var img = await image.ToImageAsync(cancellationToken);
        
        img.Mutate(x => x.Resize(200 , 200));

        var newImg = await img.ToPngAsync(cancellationToken);

        var imageEntry = _dbContext.Images.Add(new Image
        {
            Id = _guidGenerator.NewGuid(),
            Blob = newImg
        });
        
        foodTruck.HomeBanner = imageEntry.Entity;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return foodTruck.HomeBanner;
    }
    
    public async Task<bool> DeleteFoodTruckHomeBanner(Guid id,
        CancellationToken cancellationToken = default)
    {
        var foodTruck = await _dbContext.FoodTrucks
            .Include(x => x.HomeBanner)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        var image = foodTruck?.HomeBanner;
        
        if (image is null) return false;

        _dbContext.Images.Remove(image);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
    
    public async Task<Image?> UpdateFoodTruckPageBanner(Guid id,
        byte[] image,
        CancellationToken cancellationToken = default)
    {
        var foodTruck = await _dbContext.FoodTrucks
            .Include(x => x.PageBanner)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (foodTruck is null) return null;
        
        // Remove old image
        if (foodTruck.PageBanner is not null)
            _dbContext.Images.Remove(foodTruck.PageBanner);

        using var img = await image.ToImageAsync(cancellationToken);
        
        img.Mutate(x => x.Resize(1000 , 200));

        var newImg = await img.ToPngAsync(cancellationToken);

        var imageEntry = _dbContext.Images.Add(new Image
        {
            Id = _guidGenerator.NewGuid(),
            Blob = newImg
        });
        
        foodTruck.PageBanner = imageEntry.Entity;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return foodTruck.PageBanner;
    }
    
    public async Task<bool> DeleteFoodTruckPageBanner(Guid id,
        CancellationToken cancellationToken = default)
    {
        var foodTruck = await _dbContext.FoodTrucks
            .Include(x => x.PageBanner)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        var image = foodTruck?.PageBanner;
        
        if (image is null) return false;

        _dbContext.Images.Remove(image);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<FoodTruckDto?> AddFoodTruckManager(Guid id, 
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        var foodTruck = await _dbContext.FoodTrucks
            .Include(x => x.Managers)
            .Include(x => x.Products)
            .Include(x => x.Availability)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        var user = await _dbContext.Users.FindAsync([userId], cancellationToken);

        if (foodTruck is null || user is null) return null;
        
        foodTruck.Managers.Add(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return foodTruck.ToDto();
    }

    public async Task<bool> RemoveFoodTruckManager(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var foodTruck = await _dbContext.FoodTrucks
            .Include(x => x.Managers)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        var user = foodTruck?.Managers.FirstOrDefault(x => x.Id == userId);

        if (foodTruck is null || user is null) return false;
        
        foodTruck.Managers.Remove(user);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteFoodTruck(Guid id, CancellationToken cancellationToken = default)
    {
        var foodTruck = await _dbContext.FoodTrucks
            .Include(x => x.HomeBanner)
            .Include(x => x.PageBanner)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (foodTruck is null) return false;

        // Remove the home and page banner
        if (foodTruck.HomeBanner is not null)
            _dbContext.Images.Remove(foodTruck.HomeBanner);
        if (foodTruck.PageBanner is not null)
            _dbContext.Images.Remove(foodTruck.PageBanner);
        
        _dbContext.FoodTrucks.Remove(foodTruck);
        var i = await _dbContext.SaveChangesAsync(cancellationToken: cancellationToken);
        Console.WriteLine(i);
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
    Task<FoodTruckDto> CreateFoodTruck(FoodTruckRegistry foodTruckRegistry, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all food trucks
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>All food trucks</returns>
    Task<List<FoodTruckDto>> GetAllFoodTrucks(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds a food truck from a specific id
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The food truck, or null if the food truck doesn't exist</returns>
    Task<FoodTruckDto?> GetFoodTruckById(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the home banner from a food truck with a specific id
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The home banner</returns>
    Task<Image?> GetFoodTruckHomeBanner(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the page banner from a food truck with a specific id
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The page banner</returns>
    Task<Image?> GetFoodTruckPageBanner(Guid id, CancellationToken cancellationToken = default);
   
    /// <summary>
    /// Gets managers from a food truck with a specific id
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The list of managers</returns>
    Task<List<UserDto>> GetManagers(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the food truck with a specific id with the data in the food truck registry
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="foodTruckRegistry">The new data</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated food truck, or null if the food truck doesn't exist</returns>
    /// <remarks>This doesn't update the products in the food truck</remarks>
    Task<FoodTruckDto?> UpdateFoodTruck(Guid id,
        FoodTruckRegistry foodTruckRegistry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the home banner from a food truck with a specific id
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="image">The raw image in bytes</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated home banner</returns>
    /// <remarks>This also resizes the image</remarks>
    Task<Image?> UpdateFoodTruckHomeBanner(Guid id,
        byte[] image,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the home banner of the food truck with a specific id
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the image was deleted, false otherwise</returns>
    Task<bool> DeleteFoodTruckHomeBanner(Guid id,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates the page banner from a food truck with a specific id
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="image">The raw image in bytes</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The updated page banner</returns>
    /// <remarks>This also resizes the image</remarks>
    Task<Image?> UpdateFoodTruckPageBanner(Guid id,
        byte[] image,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the page banner of the food truck with a specific id
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the image was deleted, false otherwise</returns>
    Task<bool> DeleteFoodTruckPageBanner(Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a user as a manager to the food truck
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="userId">The user id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The food truck DTO</returns>
    Task<FoodTruckDto?> AddFoodTruckManager(Guid id,
        Guid userId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Removes a manager to the food truck
    /// </summary>
    /// <param name="id">The food truck id</param>
    /// <param name="userId">The user id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the manager was removed, false if the food truck or manager was not found</returns>
    Task<bool> RemoveFoodTruckManager(Guid id,
        Guid userId,
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