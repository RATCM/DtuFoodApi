using DtuFoodAPI.Models;

namespace DtuFoodAPI.DTOs;

public record FoodTruckDto
{
    public required Guid Id { get; init; }
    public required string Name { get; set; }
    public required float GpsLatitude { get; set; }
    public required float GpsLongitude { get; set; }
    public required ICollection<ProductDto> Products { get; init; } = [];
    public required ICollection<AvailabilityDto> Availability { get; init; } = [];

    public virtual bool Equals(FoodTruckDto? other)
    {
        return Id == other?.Id
               && Name == other.Name
               && Math.Abs(GpsLatitude - other.GpsLatitude) < 0.01f
               && Math.Abs(GpsLongitude - other.GpsLongitude) < 0.01f
               && Products.ToHashSet().SetEquals(other.Products.ToHashSet())
               && Availability.ToHashSet().SetEquals(other.Availability.ToHashSet());
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}