using System.Net.Http.Headers;
using System.Net.Http.Json;
using DtuFoodAPI.DTOs;

namespace DtuFoodAPI.IntegrationTests.Services;

public sealed class ProductService
{
    private readonly HttpClient _client;
    private readonly AuthService _authService;
    private JwtToken? Bearer => _authService.Bearer;

    public ProductService(HttpClient client, AuthService authService)
    {
        _client = client;
        _authService = authService;
    }

    public async Task<HttpResponseMessage> GetAllProductsFromTruck(Guid truckId)
    {
        return await _client.GetAsync($"/api/foodtruck/{truckId}/product");
    }
    
    public async Task<HttpResponseMessage> GetAllProducts(Guid truckId, string productName)
    {
        return await _client.GetAsync($"/api/foodtruck/{truckId}/product/{productName}");
    }

    public async Task<HttpResponseMessage> CreateProductForTruck(
        Guid truckId,
        ProductRegistry registry,
        JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var createProductRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/foodtruck/{truckId}/product");
        createProductRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);
        createProductRequest.Content = JsonContent.Create(registry);

        return await _client.SendAsync(createProductRequest);
    }

    public async Task<HttpResponseMessage> UpdateProductForTruck(
        Guid truckId,
        string productName,
        ProductRegistry registry,
        JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var updateProductRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/foodtruck/{truckId}/product/{productName}");
        updateProductRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);
        updateProductRequest.Content = JsonContent.Create(registry);

        return await _client.SendAsync(updateProductRequest);
    }

    public async Task<HttpResponseMessage> DeleteProductForTruck(
        Guid truckId,
        string productName,
        JwtToken? bearer = null)
    {
        bearer ??= Bearer;
        
        using var deleteProductRequest = new HttpRequestMessage(HttpMethod.Delete, $"/api/foodtruck/{truckId}/product/{productName}");
        deleteProductRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer?.AccessToken);

        return await _client.SendAsync(deleteProductRequest);
    }
}