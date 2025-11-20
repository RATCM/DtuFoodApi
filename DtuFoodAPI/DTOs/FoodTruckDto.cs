using DtuFoodAPI.Models;

namespace DtuFoodAPI.DTOs;

/// <summary>
/// The food truck object sent from endpoints
/// </summary>
public record FoodTruckDto
{
    /// <summary>
    /// The food truck id
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// The food truck name
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// The gps latitude
    /// </summary>
    public required float GpsLatitude { get; set; }
    
    /// <summary>
    /// The gps longitude
    /// </summary>
    public required float GpsLongitude { get; set; }
    
    /// <summary>
    /// The products
    /// </summary>
    public required ICollection<ProductDto> Products { get; init; } = [];
    
    /// <summary>
    /// The available time slots
    /// </summary>
    public required ICollection<AvailabilityDto> Availability { get; init; } = [];

    /// <inheritdoc />
    public virtual bool Equals(FoodTruckDto? other)
    {
        return Id == other?.Id
               && Name == other.Name
               && Math.Abs(GpsLatitude - other.GpsLatitude) < 0.01f
               && Math.Abs(GpsLongitude - other.GpsLongitude) < 0.01f
               && Products.ToHashSet().SetEquals(other.Products.ToHashSet())
               && Availability.ToHashSet().SetEquals(other.Availability.ToHashSet());
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}