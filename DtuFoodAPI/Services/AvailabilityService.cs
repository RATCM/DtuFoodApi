using System.Security.Cryptography.Xml;
using DtuFoodAPI.Database;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IDtuFoodDbContext _dbContext;

    public AvailabilityService(IDtuFoodDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AvailabilityDto?> CreateAvailabilityForTruck(Guid truckId, 
        AvailabilityRegistry registry,
        CancellationToken cancellationToken = default)
    {
        var truck = await _dbContext.FoodTrucks.Include(x => x.Availability)
            .FirstOrDefaultAsync(x => x.Id == truckId, cancellationToken);

        if (truck is null) return null;
        
        var availability = new Availability()
        {
            Truck = truck,
            DayOfWeek = Enum.Parse<WeekDay>(registry.DayOfWeek),
            OpeningTime = registry.OpeningTime,
            ClosingTime = registry.ClosingTime
        };
        truck.Availability.Add(availability);

        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return availability.ToDto();
    }
    
    public async Task<List<AvailabilityDto>?> GetAllAvailabilityForTruck(Guid truckId,
        CancellationToken cancellationToken = default)
    {
        var truck = await _dbContext.FoodTrucks
            .Include(x => x.Availability)
            .FirstOrDefaultAsync(x => x.Id == truckId, cancellationToken);

        return truck?.Availability.Select(x => x.ToDto()).ToList();
    }

    public async Task<AvailabilityDto?> GetAvailabilityForTruck(Guid truckId,
        WeekDay dayOfWeek,
        CancellationToken cancellationToken = default)
    {
        var truck = await _dbContext.FoodTrucks.Include(x => x.Availability)
            .FirstOrDefaultAsync(x => x.Id == truckId, cancellationToken);

        return truck?.Availability.FirstOrDefault(x => x.DayOfWeek == dayOfWeek)?.ToDto();
    }
    
    public async Task<AvailabilityDto?> UpdateAvailabilityForTruck(Guid truckId,
        WeekDay dayOfWeek,
        AvailabilityRegistry registry,
        CancellationToken cancellationToken = default)
    {
        var truck = await _dbContext.FoodTrucks.Include(x => x.Availability)
            .FirstOrDefaultAsync(x => x.Id == truckId, cancellationToken);

        if (truck is null) return null;

        var availability = truck.Availability.FirstOrDefault(x => x.DayOfWeek == dayOfWeek);

        if (availability is null) return null;

        truck.Availability.Remove(availability);

        var newAvailability = new Availability()
        {
            Truck = truck,
            DayOfWeek = Enum.Parse<WeekDay>(registry.DayOfWeek),
            OpeningTime = registry.OpeningTime,
            ClosingTime = registry.ClosingTime
        };
        
        truck.Availability.Add(newAvailability);
        
        await _dbContext.SaveChangesAsync(cancellationToken);

        return newAvailability.ToDto();
    }

    public async Task<bool> DeleteAvailability(Guid truckId,
        WeekDay dayOfWeek,
        CancellationToken cancellationToken = default)
    {
        var truck = await _dbContext.FoodTrucks.Include(x => x.Availability)
            .FirstOrDefaultAsync(x => x.Id == truckId, cancellationToken);
        
        if (truck is null) return false;

        var availability = truck.Availability.FirstOrDefault(x => x.DayOfWeek == dayOfWeek);

        if (availability is null) return false;


        truck.Availability.Remove(availability);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> AvailabilityExists(Guid truckId,
        WeekDay dayOfWeek,
        CancellationToken cancellationToken = default)
    {
        var truck = await _dbContext.FoodTrucks.Include(x => x.Availability)
            .FirstOrDefaultAsync(x => x.Id == truckId, cancellationToken);
        
        if (truck is null) return false;

        var hasAvailability = truck.Availability.Any(x => x.DayOfWeek == dayOfWeek);

        return hasAvailability;
    }
}

public interface IAvailabilityService
{
    Task<AvailabilityDto?> CreateAvailabilityForTruck(Guid truckId,
        AvailabilityRegistry registry,
        CancellationToken cancellationToken = default);
    
    Task<List<AvailabilityDto>?> GetAllAvailabilityForTruck(Guid truckId,
        CancellationToken cancellationToken = default);
    
    Task<AvailabilityDto?> GetAvailabilityForTruck(Guid truckId,
        WeekDay dayOfWeek,
        CancellationToken cancellationToken = default);
    
    Task<AvailabilityDto?> UpdateAvailabilityForTruck(Guid truckId,
        WeekDay dayOfWeek,
        AvailabilityRegistry registry,
        CancellationToken cancellationToken = default);
    
    Task<bool> DeleteAvailability(Guid truckId,
        WeekDay dayOfWeek,
        CancellationToken cancellationToken = default);
    
    Task<bool> AvailabilityExists(Guid truckId,
        WeekDay dayOfWeek,
        CancellationToken cancellationToken = default);
}