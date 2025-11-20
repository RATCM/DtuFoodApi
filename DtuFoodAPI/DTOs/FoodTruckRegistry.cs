namespace DtuFoodAPI.DTOs;

/// <summary>
/// Object for registering a new food truck
/// </summary>
public class FoodTruckRegistry
{
    /// <summary>
    /// The food truck name
    /// </summary>
    public required string Name { get; init; }
    
    /// <summary>
    /// The gps latitude
    /// </summary>
    public required float GpsLatitude { get; init; }
    
    /// <summary>
    /// The gps longitude
    /// </summary>
    public required float GpsLongitude { get; init; }
}