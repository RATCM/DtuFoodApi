using System.Net;
using System.Net.Http.Json;
using DtuFoodAPI.DTOs;

namespace DtuFoodAPI.IntegrationTests.FoodTruckTests;

public class GetFoodTruckTests : TestClass
{
    [Test]
    public async Task Get_AllFoodTrucks_ReturnsFoodTrucks()
    {
        // Arrange
        await AuthService.LoginAsAdmin();

        var postTruckResponse1 = await FoodTruckService.CreateFoodTruck(new FoodTruckRegistry
        {
            Name = "truck name 1",
            GpsLatitude = 0.1f,
            GpsLongitude = 0.2f
        });
        var postTruckResponse2 = await FoodTruckService.CreateFoodTruck(new FoodTruckRegistry
        {
            Name = "truck name 2",
            GpsLatitude = 0.4f,
            GpsLongitude = 0.5f
        });

        var postTruckResponseData1 = await postTruckResponse1.Content.ReadFromJsonAsync<FoodTruckDto>();
        var postTruckResponseData2 = await postTruckResponse2.Content.ReadFromJsonAsync<FoodTruckDto>();
        
        // Act
        var foodTrucksResponse = await FoodTruckService.GetAllFoodTrucks();
        var foodTrucksResponseData = await foodTrucksResponse.Content.ReadFromJsonAsync<List<FoodTruckDto>>();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(foodTrucksResponseData?.Contains(postTruckResponseData1!), Is.True);
            Assert.That(foodTrucksResponseData?.Contains(postTruckResponseData2!), Is.True);
            Assert.That(foodTrucksResponseData?.Count, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Get_FoodTruck_FailsWhenNotExists()
    {
        // Act
        var getTruckResponse = await FoodTruckService.GetFoodTruck(Guid.NewGuid());
        
        // Assert
        Assert.That(getTruckResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task Get_FoodTruck_SucceedsWhenExists()
    {
        // Arrange
        await AuthService.LoginAsAdmin();

        var postTruckResponse = await FoodTruckService.CreateFoodTruck(new FoodTruckRegistry
        {
            Name = "truck name",
            GpsLatitude = 0.1f,
            GpsLongitude = 0.2f
        });
        var postTruckResponseData = await postTruckResponse.Content.ReadFromJsonAsync<FoodTruckDto>();

        // Act
        var getTruckResponse = await FoodTruckService.GetFoodTruck(postTruckResponseData!.Id);
        var getTruckResponseData = await getTruckResponse.Content.ReadFromJsonAsync<FoodTruckDto>();
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(getTruckResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(postTruckResponseData, Is.EqualTo(getTruckResponseData));
        });
    }
}