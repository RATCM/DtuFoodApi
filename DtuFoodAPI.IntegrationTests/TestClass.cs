using System.Net.Http.Headers;
using System.Net.Http.Json;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace DtuFoodAPI.IntegrationTests;

public abstract class TestClass
{
    protected HttpClient Client;
    private TestApplicationFactory _factory;
    
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

        var user = await userService.CreateUser(registry);

        await userService.UpdateUserRole(user.Id, UserRole.Admin);
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
        return await tokenResponse.Content.ReadFromJsonAsync<JwtToken>() ??
               throw new Exception("Should not happen");
    }

    protected async Task<HttpResponseMessage> CreateUser(UserRegistry registry, JwtToken bearer)
    {
        using var postUserRequest = new HttpRequestMessage(HttpMethod.Post, "/api/user");
        postUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearer!.AccessToken!);
        postUserRequest.Content = JsonContent.Create(registry);
        
        return await Client.SendAsync(postUserRequest);
    }
}