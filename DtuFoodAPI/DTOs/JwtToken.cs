namespace DtuFoodAPI.DTOs;

public record JwtToken
{
    public string? AccessToken { get; init; }
    public string? RefreshToken { get; init; }
}