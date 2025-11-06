namespace DtuFoodAPI.DTOs;

public record ProductDto
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required decimal Price { get; init; } // We use decimal instead of float to prevent precision errors
}