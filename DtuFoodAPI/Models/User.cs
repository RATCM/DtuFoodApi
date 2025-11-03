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

    public required ICollection<FoodTruck> FoodTrucks { get; init; } = new List<FoodTruck>();

    public virtual bool Equals(User? other)
    {
        return Id == other?.Id
               && Email == other?.Email
               && PasswordHash == other?.PasswordHash
               && Role == other?.Role
               && FoodTrucks.SequenceEqual(other.FoodTrucks);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}