namespace DtuFoodAPI.DTOs;

/// <summary>
/// The Availability object sent from endpoints
/// </summary>
public record AvailabilityDto
{
    /// <summary>
    /// The day of the week
    /// </summary>
    public required string DayOfWeek { get; init; }
    
    /// <summary>
    /// The opening time
    /// </summary>
    public required TimeOnly OpeningTime { get; set; }
    
    /// <summary>
    /// The closing time
    /// </summary>
    public required TimeOnly ClosingTime { get; set; }
}