namespace DtuFoodAPI.DTOs;

public record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; set; }
    public required string Role { get; init; }
    public required ICollection<Guid> FoodTrucks { get; init; } = [];
    
    public virtual bool Equals(UserDto? other)
    {
        return Id == other?.Id
               && Email == other?.Email
               && Role == other?.Role
               && FoodTrucks.SequenceEqual(other.FoodTrucks);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}