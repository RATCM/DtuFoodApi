namespace DtuFoodAPI.DTOs;

/// <summary>
/// Object for registering a new food truck manager
/// </summary>
public class FoodTruckManagerRegistry
{
    /// <summary>
    /// The user id
    /// </summary>
    public required Guid Id { get; init; }
    
    /// <summary>
    /// The user email
    /// </summary>
    public required string Email { get; set; }
}