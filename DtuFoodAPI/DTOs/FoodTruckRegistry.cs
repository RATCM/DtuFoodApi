namespace DtuFoodAPI.DTOs;

public class FoodTruckRegistry
{
    public required string Name { get; init; }
    public required float GpsLatitude { get; init; }
    public required float GpsLongitude { get; init; }
}