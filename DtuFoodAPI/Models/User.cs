using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DtuFoodAPI.Models;

[Table("Users")]
[Index(nameof(Email), IsUnique = true)]
public record User
{
    [Key]
    public required Guid Id { get; init; }
    
    [MaxLength(128)]
    public required string Email { get; set; }
    
    // I don't know how large the hash can be
    public required string PasswordHash { get; set; }
    
    public required UserRole Role { get; set; }
    
    public required ICollection<FoodTruck> FoodTrucks { get; init; }
}