using System.Net.Http.Headers;
using System.Net.Http.Json;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.IntegrationTests.Services;
using DtuFoodAPI.Models;
using DtuFoodAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using FoodTruckService = DtuFoodAPI.IntegrationTests.Services.FoodTruckService;
using ProductService = DtuFoodAPI.IntegrationTests.Services.ProductService;
using UserService = DtuFoodAPI.IntegrationTests.Services.UserService;

namespace DtuFoodAPI.IntegrationTests;

public abstract class TestClass
{
    protected HttpClient Client;
    private TestApplicationFactory _factory;
    protected UserDto Admin { get; private set; }

    protected AuthService AuthService;
    protected FoodTruckService FoodTruckService;
    protected ProductService ProductService;
    protected UserService UserService;
    
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
            Password = "AnAdminPassword1!"
        };

        Admin = await userService.CreateUser(registry);

        await userService.UpdateUserRole(Admin.Id, UserRole.Admin);

        AuthService = new AuthService(Client);
        FoodTruckService = new FoodTruckService(Client, AuthService);
        ProductService = new ProductService(Client, AuthService);
        UserService = new UserService(Client, AuthService);
        
    }

    [TearDown]
    public async Task TearDown()
    {
        await _factory.DisposeAsync();
        Client.Dispose();
    }
}