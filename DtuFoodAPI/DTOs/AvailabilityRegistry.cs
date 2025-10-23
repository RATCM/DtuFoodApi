using DtuFoodAPI.Models;

namespace DtuFoodAPI.DTOs;

public class AvailabilityRegistry
{
    // Enums will only handle integers in the registry
    // so it probably makes more sense to handle it as
    // a string from the client-end, and then
    // convert it to an enum
    public required string DayOfWeek { get; init; }
    public required TimeOnly OpeningTime { get; init; }
    public required TimeOnly ClosingTime { get; init; }
}