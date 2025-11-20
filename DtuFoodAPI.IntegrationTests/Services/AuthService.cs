using System.Net.Http.Headers;
using System.Net.Http.Json;
using DtuFoodAPI.DTOs;

namespace DtuFoodAPI.IntegrationTests.Services;

public sealed class AuthService
{
    private readonly HttpClient _client;
    public JwtToken? Bearer { get; private set; }

    public AuthService(HttpClient client)
    {
        _client = client;
    }
    
    public async Task<JwtToken> LoginAsAdmin()
    {
        var tokenResponse = await _client.PostAsJsonAsync("/api/auth/login", new UserRegistry()
        {
            Email = "admin@test",
            Password = "AnAdminPassword1!"
        });

        Bearer = await tokenResponse.Content.ReadFromJsonAsync<JwtToken>();
        return Bearer ??
               throw new Exception("Should not happen");
    }
    
    public async Task<JwtToken?> LoginUser(UserRegistry userRegistry)
    {
        var tokenResponse = await _client.PostAsJsonAsync("/api/auth/login", userRegistry);
        
        Bearer = await tokenResponse.Content.ReadFromJsonAsync<JwtToken>();
        return Bearer;
    }

    public async Task<JwtToken?> Refresh(JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var refreshRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        refreshRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.RefreshToken);

        var response = await _client.SendAsync(refreshRequest);

        Bearer = await response.Content.ReadFromJsonAsync<JwtToken>();
        return Bearer;
    }
}