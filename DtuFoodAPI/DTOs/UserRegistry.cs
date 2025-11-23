using DtuFoodAPI.Models;

namespace DtuFoodAPI.DTOs;

public class UserRegistry
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string UserRole { get; init; } = "Manager";
}