using System.Net;
using System.Net.Http.Headers;
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
    public async Task Get_AllUsers_SucceedsWhenUnauthorized()
    {
        // Arrange
        var token = await LoginAsAdmin();
        using var getUsersRequest = new HttpRequestMessage(HttpMethod.Get, "/api/user");
        getUsersRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken!);
        
        // Act
        var response = await Client.SendAsync(getUsersRequest);

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
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
        var token = await LoginAsAdmin();
        
        var registry = new UserRegistry() { Email = "some@email", Password = "some password" };
        var userResponse = await CreateUser(registry, token);
        var data = await userResponse.Content.ReadFromJsonAsync<UserDto>();
        
        // Act
        var response = await Client.GetAsync($"/api/user/{data!.Id}");
        var responseData = await response.Content.ReadFromJsonAsync<UserDto>();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(userResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(data, Is.EqualTo(responseData));
        });
    }
}