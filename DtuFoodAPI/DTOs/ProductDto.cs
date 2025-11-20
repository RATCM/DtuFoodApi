namespace DtuFoodAPI.DTOs;

public record ProductDto
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    // We use string instead of decimal to make it compatible with the frontend
    public required string Price { get; init; } 
    public required string? Category { get; init; }
}