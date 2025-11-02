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
    }

    [TearDown]
    public async Task TearDown()
    {
        await _factory.DisposeAsync();
        Client.Dispose();
    }
}