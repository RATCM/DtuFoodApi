using System.ComponentModel.DataAnnotations;
using DtuFoodAPI.Models;

namespace DtuFoodAPI.DTOs;

public class UserRegistry
{
    [Required]
    [EmailAddress(ErrorMessage = "Invalid emailaddress")]
    public required string Email { get; init; }
    
    [Required]
    [StringLength(100, MinimumLength = 4, ErrorMessage = "Password must be at least 4 character")]
    public required string Password { get; init; }
}