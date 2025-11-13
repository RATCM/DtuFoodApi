using System.ComponentModel.DataAnnotations;

namespace DtuFoodAPI.DTOs;

public class FoodTruckManagerRegistry
{
    [Required]
    public required Guid Id { get; init; }
    
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string Email { get; set; }
}