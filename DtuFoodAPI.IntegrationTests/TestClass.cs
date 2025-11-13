using System.Net.Http.Headers;
using System.Net.Http.Json;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DtuFoodAPI.IntegrationTests;

public abstract class TestClass
{
    protected HttpClient Client;
    private TestApplicationFactory _factory;
    private JwtToken? _bearer;
    protected UserDto Admin { get; private set; }
    
    [SetUp]
    public async Task SetUp()
    {
        _factory = new TestApplicationFactory();
        Client = _factory.CreateClient();
        using var scope = _factory.Services.CreateScope();

        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<TestDbContext>();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        
        var userService = services.GetRequiredService<IUserService>();

        // We create a sample admin user for testing
        var registry = new UserRegistry()
        {
            Email = "admin@test",
            Password = "admin"
        };

        Admin = await userService.CreateUser(registry);

        await userService.UpdateUserRole(Admin.Id, UserRole.Admin);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _factory.DisposeAsync();
        Client.Dispose();
    }

    protected async Task<JwtToken> LoginAsAdmin()
    {
        var tokenResponse = await Client.PostAsJsonAsync("/api/auth/login", new UserRegistry()
        {
            Email = "admin@test",
            Password = "admin"
        });

        _bearer = await tokenResponse.Content.ReadFromJsonAsync<JwtToken>();
        return _bearer ??
               throw new Exception("Should not happen");
    }

    protected async Task<HttpResponseMessage> CreateUser(UserRegistry registry)
    {
        using var postUserRequest = new HttpRequestMessage(HttpMethod.Post, "/api/user");
        postUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearer!.AccessToken!);
        postUserRequest.Content = JsonContent.Create(registry);
        
        return await Client.SendAsync(postUserRequest);
    }

    protected async Task<HttpResponseMessage> CreateFoodTruck(FoodTruckRegistry registry)
    {
        using var postTruckRequest = new HttpRequestMessage(HttpMethod.Post, "/api/foodtruck");
        postTruckRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearer!.AccessToken!);
        postTruckRequest.Content = JsonContent.Create(registry);

        return await Client.SendAsync(postTruckRequest);
    }
    
    protected async Task<HttpResponseMessage> SetFoodTruckHomeBanner(Guid id, byte[] file)
    {
        using var content = new MultipartFormDataContent();
        using var ms = new MemoryStream(file);
        using var setHomeBannerRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/foodtruck/{id}/image/home");
        var fileContent = new StreamContent(ms);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        
        content.Add(fileContent, "file", "file-name");
        
        setHomeBannerRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearer!.AccessToken!);
        setHomeBannerRequest.Content = content;
        
        return await Client.SendAsync(setHomeBannerRequest);
    }
    
    protected async Task<HttpResponseMessage> SetFoodTruckPageBanner(Guid id, byte[] file)
    {
        using var content = new MultipartFormDataContent();
        using var ms = new MemoryStream(file);
        using var setHomeBannerRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/foodtruck/{id}/image/page");
        var fileContent = new StreamContent(ms);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        
        content.Add(fileContent, "file", "file-name");
        
        setHomeBannerRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearer!.AccessToken!);
        setHomeBannerRequest.Content = content;
        
        return await Client.SendAsync(setHomeBannerRequest);
    }

    protected async Task<HttpResponseMessage> AddFoodTruckManager(Guid id, FoodTruckManagerRegistry registry)
    {
        using var addManagerRequest = new HttpRequestMessage(HttpMethod.Put, $"/api/foodtruck/{id}/manager");
        addManagerRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _bearer!.AccessToken!);
        addManagerRequest.Content = JsonContent.Create(registry);

        return await Client.SendAsync(addManagerRequest);
    }
}