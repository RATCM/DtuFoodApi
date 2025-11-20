namespace DtuFoodAPI.DTOs;

public class ProductRegistry
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string Price { get; init; } // We use decimal instead of float to prevent precision errors
    public string? Category { get; init; }
}