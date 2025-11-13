using System.ComponentModel.DataAnnotations;

namespace DtuFoodAPI.DTOs;

public class ProductRegistry
{
    [Required]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Product name must be 2–100 characters.")]
    public required string Name { get; init; }
    
    [Required]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Description must be 5–500 characters.")]
    public required string Description { get; init; }
    
    [Required]
    [Range(0.01, 10000.00, ErrorMessage = "Price must not be less than 0.")]
    public required decimal Price { get; init; } // We use decimal instead of float to prevent precision errors
    
    [StringLength(50, ErrorMessage = "Category must be at most 50 characters.")]
    public string? Category { get; init; }
}