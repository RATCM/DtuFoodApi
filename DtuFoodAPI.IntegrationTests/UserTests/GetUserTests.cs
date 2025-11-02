using System.Net;
using System.Net.Http.Json;
using DtuFoodAPI.DTOs;
using DtuFoodAPI.Models;

namespace DtuFoodAPI.IntegrationTests.UserTests;

public class GetUserTests : TestClass
{
    [Test]
    public async Task Get_AllUsers_FailsWhenUnauthorized()
    {
        // Act
        var response = await Client.GetAsync("/api/user");

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Get_UserById_FailsWhenUserNotExists()
    {
        // Act
        var response = await Client.GetAsync($"/api/user/{Guid.NewGuid()}");

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Get_UserById_SucceedsWhenUserExists()
    {
        // Arrange
        var registry = new UserRegistry() { Email = "some@email", Password = "some password" };
        var userResponse = await Client.PostAsJsonAsync("/api/user", registry);
        var data = await userResponse.Content.ReadFromJsonAsync<User>();
        
        // Act
        var response = await Client.GetAsync($"/api/user/{data!.Id}");
        var responseData = await response.Content.ReadFromJsonAsync<User>();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(userResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(data, Is.EqualTo(responseData));
        });
    }
}