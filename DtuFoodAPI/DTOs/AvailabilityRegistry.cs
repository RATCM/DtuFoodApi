using DtuFoodAPI.Models;

namespace DtuFoodAPI.DTOs;

/// <summary>
/// The Availability object received in endpoints
/// </summary>
public class AvailabilityRegistry
{
    /// <summary>
    /// The day of the week
    /// </summary>
    public required string DayOfWeek { get; init; }
    
    /// <summary>
    /// The opening time
    /// </summary>
    public required TimeOnly OpeningTime { get; init; }
    
    /// <summary>
    /// The closing time
    /// </summary>
    public required TimeOnly ClosingTime { get; init; }
}