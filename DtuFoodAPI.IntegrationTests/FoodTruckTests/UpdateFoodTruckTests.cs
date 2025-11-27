using System.Net;
using System.Net.Http.Json;
using DtuFoodAPI.DTOs;

namespace DtuFoodAPI.IntegrationTests.FoodTruckTests;

public class UpdateFoodTruckTests : TestClass
{
    [Test]
    [Description("Food truck cannot be updated from non-managers")]
    public async Task Put_FoodTruck_FailsWhenUnauthorized()
    {
        // Arrange
        await AuthService.LoginAsAdmin();

        // Managers are manually set, so this truck has no managers
        var createTruckResponse = await FoodTruckService.CreateFoodTruck(new FoodTruckRegistry()
        {
            Name = "some name",
            GpsLatitude = 0.1f,
            GpsLongitude = 0.2f
        });
        var createTruckResponseData = await createTruckResponse.Content.ReadFromJsonAsync<FoodTruckDto>();
        
        // Act
        var updateTruckResponse = await FoodTruckService.UpdateFoodTruck(
            createTruckResponseData!.Id,
            new FoodTruckRegistry()
            {
                Name = "updated name",
                GpsLatitude = 0.3f,
                GpsLongitude = 0.4f
            }
        );
        
        // Assert
        Assert.That(updateTruckResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }
    
    [Test]
    public async Task Put_FoodTruck_FailsWhenNotAuthenticated()
    {
        // Arrange
        await AuthService.LoginAsAdmin();

        // Managers are manually set, so this truck has no managers
        var createTruckResponse = await FoodTruckService.CreateFoodTruck(new FoodTruckRegistry()
        {
            Name = "some name",
            GpsLatitude = 0.1f,
            GpsLongitude = 0.2f
        });
        var createTruckResponseData = await createTruckResponse.Content.ReadFromJsonAsync<FoodTruckDto>();
        
        // Act
        var updateTruckResponse = await FoodTruckService.UpdateFoodTruck(
            createTruckResponseData!.Id,
            new FoodTruckRegistry()
            {
                Name = "updated name",
                GpsLatitude = 0.3f,
                GpsLongitude = 0.4f
            },
            bearer: new JwtToken() // passing an empty jwt token is essentially the same as logging out
        );
        
        // Assert
        Assert.That(updateTruckResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Put_FoodTruck_FailsWhenNotExists()
    {
        // Arrange
        await AuthService.LoginAsAdmin();

        var createTruckResponse = await FoodTruckService.CreateFoodTruck(new FoodTruckRegistry()
        {
            Name = "some name",
            GpsLatitude = 0.1f,
            GpsLongitude = 0.2f
        });
        var createTruckResponseData = await createTruckResponse.Content.ReadFromJsonAsync<FoodTruckDto>();
        
        // Add admin as manager
        await FoodTruckService.AddFoodTruckManager(createTruckResponseData!.Id, Admin.Id);
        
        var updatedTruckRegistry = new FoodTruckRegistry()
        {
            Name = "updated name",
            GpsLatitude = 0.3f,
            GpsLongitude = 0.4f
        };
        
        // Act
        var updateTruckResponse = await FoodTruckService.UpdateFoodTruck(
            Guid.NewGuid(), updatedTruckRegistry);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(updateTruckResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }
    
    [Test]
    public async Task Put_FoodTruck_SucceedsWhenManager()
    {
        // Arrange
        await AuthService.LoginAsAdmin();

        var createTruckResponse = await FoodTruckService.CreateFoodTruck(new FoodTruckRegistry()
        {
            Name = "some name",
            GpsLatitude = 0.1f,
            GpsLongitude = 0.2f
        });
        var createTruckResponseData = await createTruckResponse.Content.ReadFromJsonAsync<FoodTruckDto>();
        
        // Add admin as manager
        await FoodTruckService.AddFoodTruckManager(createTruckResponseData!.Id, Admin.Id);
        
        var updatedTruckRegistry = new FoodTruckRegistry()
        {
            Name = "updated name",
            GpsLatitude = 0.3f,
            GpsLongitude = 0.4f
        };

        var expectedUpdatedTruck = new FoodTruckDto()
        {
            Id = createTruckResponseData.Id,
            Name = updatedTruckRegistry.Name,
            GpsLatitude = updatedTruckRegistry.GpsLatitude,
            GpsLongitude = updatedTruckRegistry.GpsLongitude,
            Availability = [],
            Products = []
        };
        
        // Act
        var updateTruckResponse = await FoodTruckService.UpdateFoodTruck(
            createTruckResponseData!.Id, updatedTruckRegistry);
        var updateTruckResponseData = await updateTruckResponse.Content.ReadFromJsonAsync<FoodTruckDto>();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(updateTruckResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(expectedUpdatedTruck, Is.EqualTo(updateTruckResponseData));
        });
    }
}