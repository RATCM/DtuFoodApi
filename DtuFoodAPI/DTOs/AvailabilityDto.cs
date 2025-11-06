namespace DtuFoodAPI.DTOs;

public record AvailabilityDto
{
    public required string DayOfWeek { get; init; }
    public required TimeOnly OpeningTime { get; set; }
    public required TimeOnly ClosingTime { get; set; }
}