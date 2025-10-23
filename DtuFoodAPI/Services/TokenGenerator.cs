using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DtuFoodAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace DtuFoodAPI.Services;

public class TokenGenerator : ITokenGenerator
{
    private readonly IGuidGenerator _guidGenerator;
    private readonly IConfiguration _configuration;
    
    public TokenGenerator(IGuidGenerator guidGenerator, IConfiguration configuration)
    {
        _guidGenerator = guidGenerator;
        _configuration = configuration;
    }
    
    public string GenerateJwtAccessToken(User user)
    {
        var scheme = _configuration.GetSection("Authentication:Schemes:Access");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("role", user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, _guidGenerator.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Convert.FromBase64String(
            scheme["SigningKeys:0:Value"]!));

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);
        var token = new JwtSecurityToken(
            issuer: scheme["ValidIssuer"]!,
            audience: scheme["Audience"]!,
            claims: claims,
            expires: DateTime.Now.Add(TimeSpan.Parse(scheme["TimeValid"]!)),
            signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateJwtRefreshToken(User user)
    {
        var scheme = _configuration.GetSection("Authentication:Schemes:Refresh");
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, _guidGenerator.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Convert.FromBase64String(
            scheme["SigningKeys:0:Value"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var token = new JwtSecurityToken(
            issuer: scheme["ValidIssuer"],
            audience: scheme["Audience"],
            claims: claims,
            expires: DateTime.Now.Add(TimeSpan.Parse(scheme["TimeValid"]!)),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public interface ITokenGenerator
{
    /// <summary>
    /// Generates a new access token for a user
    /// </summary>
    /// <param name="user">The user</param>
    /// <returns>The raw access token</returns>
    string GenerateJwtAccessToken(User user);
    
    /// <summary>
    /// Generates a new refresh token for a user
    /// </summary>
    /// <param name="user">The user</param>
    /// <returns>The raw refresh token</returns>
    string GenerateJwtRefreshToken(User user);
}