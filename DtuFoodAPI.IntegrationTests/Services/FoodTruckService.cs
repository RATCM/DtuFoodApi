using System.Net.Http.Headers;
using System.Net.Http.Json;
using DtuFoodAPI.DTOs;

namespace DtuFoodAPI.IntegrationTests.Services;

public sealed class FoodTruckService
{
    private readonly HttpClient _client;
    private readonly AuthService _authService;
    private JwtToken? Bearer => _authService.Bearer;
    public FoodTruckService(HttpClient client, AuthService authService)
    {
        _client = client;
        _authService = authService;
    }

    public async Task<HttpResponseMessage> GetAllFoodTrucks()
    {
        return await _client.GetAsync("/api/foodtruck");
    }

    public async Task<HttpResponseMessage> GetFoodTruck(Guid id)
    {
        return await _client.GetAsync($"/api/foodtruck/{id}");
    }

    public async Task<HttpResponseMessage> UpdateFoodTruck(Guid id, FoodTruckRegistry registry, JwtToken? bearer = null)
    {
        bearer ??= Bearer;

        using var putTruckRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/foodtruck/{id}");
        putTruckRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);
        putTruckRequest.Content = JsonContent.Create(registry);

        return await _client.SendAsync(putTruckRequest);
    }

    public async Task<HttpResponseMessage> CreateFoodTruck(FoodTruckRegistry registry, JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var postTruckRequest = new HttpRequestMessage(HttpMethod.Post, "/api/foodtruck");
        postTruckRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);
        postTruckRequest.Content = JsonContent.Create(registry);

        return await _client.SendAsync(postTruckRequest);
    }

    public async Task<HttpResponseMessage> DeleteFoodTruck(Guid id, JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var deleteTruckRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/foodtruck/{id}");
        deleteTruckRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);

        return await _client.SendAsync(deleteTruckRequest);
    }
    
    public async Task<HttpResponseMessage> SetFoodTruckHomeBanner(Guid id, byte[] file, JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var content = new MultipartFormDataContent();
        using var ms = new MemoryStream(file);
        using var setHomeBannerRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/foodtruck/{id}/image/home");
        var fileContent = new StreamContent(ms);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        
        content.Add(fileContent, "file", "file-name");
        
        setHomeBannerRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);
        setHomeBannerRequest.Content = content;
        
        return await _client.SendAsync(setHomeBannerRequest);
    }

    public async Task<HttpResponseMessage> GetFoodTruckHomeBanner(Guid id)
    {
        return await _client.GetAsync($"/api/foodtruck/{id}/image/home");
    }
    
    public async Task<HttpResponseMessage> SetFoodTruckPageBanner(Guid id, byte[] file, JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var content = new MultipartFormDataContent();
        using var ms = new MemoryStream(file);
        using var setHomeBannerRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/foodtruck/{id}/image/page");
        var fileContent = new StreamContent(ms);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        
        content.Add(fileContent, "file", "file-name");
        
        setHomeBannerRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);
        setHomeBannerRequest.Content = content;
        
        return await _client.SendAsync(setHomeBannerRequest);
    }
    
    public async Task<HttpResponseMessage> GetFoodTruckPageBanner(Guid id)
    {
        return await _client.GetAsync($"/api/foodtruck/{id}/image/page");
    }
    
    public async Task<HttpResponseMessage> AddFoodTruckManager(Guid id, FoodTruckManagerRegistry registry, JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var addManagerRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/foodtruck/{id}/manager");
        addManagerRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);
        addManagerRequest.Content = JsonContent.Create(registry);

        return await _client.SendAsync(addManagerRequest);
    }

}