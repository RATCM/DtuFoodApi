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
        var response = await UserService.GetAllUsers();

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task Get_AllUsers_SucceedsWhenUnauthorized()
    {
        // Arrange
        var token = await AuthService.LoginAsAdmin();
        //var token = await LoginAsAdmin();
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
        var response = await UserService.GetUser(Guid.NewGuid());
        //var response = await Client.GetAsync($"/api/user/{Guid.NewGuid()}");

        //Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Get_UserById_SucceedsWhenUserExists()
    {
        // Arrange
        await AuthService.LoginAsAdmin();
        
        var registry = new UserRegistry() { Email = "some@email", Password = "Some password1!" };
        var userResponse = await UserService.CreateUser(registry);
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

    [Test]
    public async Task Get_AllUsers_FailsWhenRateLimitExceeded()
    {
        // Arrange
        var jwtToken = await AuthService.LoginAsAdmin();
        //var jwtToken = await LoginAsAdmin();
        List<HttpResponseMessage> responses = [];
        
        // Act
        for (int i = 0; i < 11; i++)
        {
            using var getUsersRequest = new HttpRequestMessage(HttpMethod.Get, "/api/user");
            getUsersRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken!.AccessToken!);

            responses.Add(await Client.SendAsync(getUsersRequest));
        }
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(responses.Take(10).All(x => x.StatusCode == HttpStatusCode.OK), Is.True);
            Assert.That(responses.Last().StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        });
    }
    
    [Test]
    public async Task Get_UserById_FailsWhenRateLimitExceeded()
    {
        // Arrange
        var adminId = Admin.Id;
        List<HttpResponseMessage> responses = [];
        
        // Act
        for (int i = 0; i < 31; i++)
        {
            responses.Add(await UserService.GetUser(adminId));
            //responses.Add(await Client.GetAsync($"/api/user/{adminId}"));
        }
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(responses.Take(30).All(x => x.StatusCode == HttpStatusCode.OK), Is.True);
            Assert.That(responses.Last().StatusCode, Is.EqualTo(HttpStatusCode.TooManyRequests));
        });
    }
}