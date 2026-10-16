using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class EventTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EventTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndGetToken(string role)
    {
        var email =
            $"{role.ToLower()}-{Guid.NewGuid()}@test.com";

        var response = await _client.PostAsJsonAsync(
            "/api/Auth/register",
            new
            {
                firstName = "Test",
                lastName = role,
                email,
                password = "password123!",
                role
            });

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        return json.GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task Organiser_CanCreateEvent()
    {
        var token = await RegisterAndGetToken("Organiser");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync(
            "/api/Events",
            new
            {
                name = "Test 10K",
                description = "Automated test event",
                eventDate = "2026-12-01T07:00:00",
                location = "Cape Town",
                distanceKm = 10,
                eventType = "Running"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotCreateEvent()
    {
        var token = await RegisterAndGetToken("Participant");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync(
            "/api/Events",
            new
            {
                name = "Forbidden Event",
                description = "Should not be created",
                eventDate = "2026-12-01T07:00:00",
                location = "Cape Town",
                distanceKm = 5,
                eventType = "Running"
            });

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedUser_CanViewEvents()
    {
        var token = await RegisterAndGetToken("Participant");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.GetAsync("/api/Events");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}