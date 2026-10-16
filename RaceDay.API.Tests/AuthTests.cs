using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ValidParticipant_ReturnsOk()
    {
        var request = new
        {
            firstName = "Test",
            lastName = "Participant",
            email = "participant1@test.com",
            password = "password123!",
            role = "Participant",
            phone = "0711111111",
            dateOfBirth = "2000-01-01T00:00:00"
        };

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_ValidUser_ReturnsOk()
    {
        var email = $"login-{Guid.NewGuid()}@test.com";

        var registerRequest = new
        {
            firstName = "Login",
            lastName = "Test",
            email,
            password = "password123!",
            role = "Participant",
            phone = "0722222222",
            dateOfBirth = "2000-01-01T00:00:00"
        };

        var registerResponse = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            registerRequest);

        Assert.Equal(
            HttpStatusCode.OK,
            registerResponse.StatusCode);

        var loginRequest = new
        {
            email,
            password = "password123!"
        };

        var loginResponse = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            loginRequest);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsUnauthorized()
    {
        var email = $"wrong-{Guid.NewGuid()}@test.com";

        await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new
            {
                firstName = "Wrong",
                lastName = "Password",
                email,
                password = "password123!",
                role = "Participant"
            });

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/login",
            new
            {
                email,
                password = "wrongpassword"
            });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }
}