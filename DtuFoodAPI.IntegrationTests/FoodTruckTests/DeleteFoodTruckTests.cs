using System.Net;
using System.Net.Http.Json;
using DtuFoodAPI.DTOs;

namespace DtuFoodAPI.IntegrationTests.FoodTruckTests;

public class DeleteFoodTruckTests : TestClass
{
    [Test]
    [Description("401 Unauthorized when bearer token not provided")]
    public async Task Delete_FoodTruck_FailsWhenUnauthorized()
    {
        // Arrange
        await AuthService.LoginAsAdmin();

        var createTruckResponse = await FoodTruckService.CreateFoodTruck(new FoodTruckRegistry()
        {
            Name = "some name",
            GpsLatitude = 0.2f,
            GpsLongitude = 0.3f
        });
        var createTruckResponseData = await createTruckResponse.Content.ReadFromJsonAsync<FoodTruckDto>();
        
        // Act
        var deleteTruckResponse = await FoodTruckService
            .DeleteFoodTruck(createTruckResponseData!.Id, bearer: new JwtToken());

        // Assert
        Assert.That(deleteTruckResponse.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    [Description("403 Forbidden when user is not admin")]
    public async Task Delete_FoodTruck_FailsWhenNotAuthenticated()
    {
        // Arrange
        // We create a sample user first
        await AuthService.LoginAsAdmin();
        var sampleUser = new UserRegistry()
        {
            Email = "some@mail.com",
            Password = "Password1!"
        };
        
        var createUserResponse = await UserService.CreateUser(sampleUser);
        var createUserResponseData = await createUserResponse.Content.ReadFromJsonAsync<UserDto>();
        
        var createTruckResponse = await FoodTruckService.CreateFoodTruck(new FoodTruckRegistry()
        {
            Name = "some name",
            GpsLatitude = 0.2f,
            GpsLongitude = 0.3f
        });
        var createTruckResponseData = await createTruckResponse.Content.ReadFromJsonAsync<FoodTruckDto>();
        
        // Make the sample user a manager for good measure
        await FoodTruckService.AddFoodTruckManager(createTruckResponseData!.Id, createUserResponseData!.Id);
        
        // Login as the sample user
        await AuthService.LoginUser(sampleUser);

        // Act
        var deleteTruckResponse = await FoodTruckService.DeleteFoodTruck(createTruckResponseData!.Id);

        // Assert
        Assert.That(deleteTruckResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
    }

    [Test]
    [Description("404 Not Found when food truck doesn't exist")]
    public async Task Delete_FoodTruck_FailsWhenNotExists()
    {
        // Arrange
        await AuthService.LoginAsAdmin();
        
        await FoodTruckService.CreateFoodTruck(new FoodTruckRegistry()
        {
            Name = "some name",
            GpsLatitude = 0.2f,
            GpsLongitude = 0.3f
        });
        
        // Act
        var deleteTruckResponse = await FoodTruckService
            .DeleteFoodTruck(Guid.NewGuid());

        // Assert
        Assert.That(deleteTruckResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    [Description("204 No Content when food truck is updated")]
    public async Task Delete_FoodTruck_SucceedsWhenExists()
    {
        // Arrange
        await AuthService.LoginAsAdmin();

        var createTruckResponse = await FoodTruckService.CreateFoodTruck(new FoodTruckRegistry()
        {
            Name = "some name",
            GpsLatitude = 0.2f,
            GpsLongitude = 0.3f
        });
        var createTruckResponseData = await createTruckResponse.Content.ReadFromJsonAsync<FoodTruckDto>();
        
        // Act
        var deleteTruckResponse = await FoodTruckService
            .DeleteFoodTruck(createTruckResponseData!.Id);
        
        // Make sure its actually deleted
        var deleteTruckResponseCheck = await FoodTruckService
            .DeleteFoodTruck(createTruckResponseData!.Id);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(deleteTruckResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(deleteTruckResponseCheck.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }
    
}