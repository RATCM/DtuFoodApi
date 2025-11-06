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
}