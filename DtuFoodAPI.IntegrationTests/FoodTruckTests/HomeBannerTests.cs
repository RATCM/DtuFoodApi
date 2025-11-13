using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using DtuFoodAPI.DTOs;
using Microsoft.AspNetCore.Http;

namespace DtuFoodAPI.IntegrationTests.FoodTruckTests;

public class HomeBannerTests : TestClass
{
    [SetUp]
    public new async Task SetUp()
    {
        await base.SetUp();

        await LoginAsAdmin();
    }
    
    [Test]
    public async Task Get_HomeBanner_FailsWhenNotExists()
    {
        // Arrange
        var truckRegistry = new FoodTruckRegistry()
        {
            Name = "some name",
            GpsLatitude = 0.1f,
            GpsLongitude = 0.2f,
        };

        var postTruckResponse = await CreateFoodTruck(truckRegistry);
        var postTruckResponseData = await postTruckResponse.Content.ReadFromJsonAsync<FoodTruckDto>();
        
        // Act
        var response = await Client.GetAsync($"/api/foodtruck/{postTruckResponseData!.Id}/image/home");

        //Assert
        Assert.Multiple(() =>
        {
            Assert.That(postTruckResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task Get_HomeBanner_SucceedsWhenExists()
    {
        // Arrange
        await LoginAsAdmin();
        var truckRegistry = new FoodTruckRegistry()
        {
            Name = "some name",
            GpsLatitude = 0.1f,
            GpsLongitude = 0.2f,
        };
        var imageData = "some raw image data"u8.ToArray();

        var postTruckResponse = await CreateFoodTruck(truckRegistry);
        var postTruckResponseData = await postTruckResponse.Content.ReadFromJsonAsync<FoodTruckDto>();

        // We add ourselves as a manager
        var addManagerResponse = await AddFoodTruckManager(postTruckResponseData!.Id, new FoodTruckManagerRegistry()
        {
            Id = Admin.Id,
            Email = Admin.Email
        });
        
        var putImageResponse = await SetFoodTruckHomeBanner(postTruckResponseData!.Id, imageData);

        // Act
        var getImageResponse = await Client.GetAsync($"/api/foodtruck/{postTruckResponseData!.Id}/image/home");

        var getImageResponseData = await getImageResponse.Content.ReadAsByteArrayAsync();
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(addManagerResponse.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(putImageResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(getImageResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getImageResponseData, Is.EqualTo(imageData));
        });
        
    }

}