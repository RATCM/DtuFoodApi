using System.Net.Http.Headers;
using System.Net.Http.Json;
using DtuFoodAPI.DTOs;

namespace DtuFoodAPI.IntegrationTests.Services;

public sealed class UserService
{
    private readonly HttpClient _client;
    private readonly AuthService _authService;
    private JwtToken? Bearer => _authService.Bearer;

    public UserService(HttpClient client, AuthService authService)
    {
        _client = client;
        _authService = authService;
    }
    
    public async Task<HttpResponseMessage> CreateUser(UserRegistry registry, JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var postUserRequest = new HttpRequestMessage(HttpMethod.Post, "/api/user");
        postUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);
        postUserRequest.Content = JsonContent.Create(registry);
        
        return await _client.SendAsync(postUserRequest);
    }

    public async Task<HttpResponseMessage> GetAllUsers(JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var getUserRequest = new HttpRequestMessage(HttpMethod.Get, "/api/user");
        getUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);
        
        return await _client.SendAsync(getUserRequest);
    }

    public async Task<HttpResponseMessage> GetUser(Guid id)
    {
        return await _client.GetAsync($"/api/user/{id}");
    }

    public async Task<HttpResponseMessage> UpdateUser(Guid id, UserRegistry registry, JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var putUserRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/user/{id}");
        putUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);
        putUserRequest.Content = JsonContent.Create(registry);
        
        return await _client.SendAsync(putUserRequest);
    }

    public async Task<HttpResponseMessage> DeleteUser(Guid id, JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var deleteUserRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/user/{id}");
        deleteUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);
        
        return await _client.SendAsync(deleteUserRequest);
    }
}