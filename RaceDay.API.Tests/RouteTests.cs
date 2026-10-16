using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class RouteTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RouteTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndGetToken(string role)
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var email = $"{role.ToLower()}-{Guid.NewGuid()}@test.com";

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

        var json =
            await response.Content.ReadFromJsonAsync<JsonElement>();

        return json.GetProperty("token").GetString()!;
    }

    private async Task<int> CreateEventAsOrganiser()
    {
        var organiserToken =
            await RegisterAndGetToken("Organiser");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                organiserToken);

        var response = await _client.PostAsJsonAsync(
            "/api/Events",
            new
            {
                name = "Route Test 10K",
                description = "Event used for route testing",
                eventDate = "2026-12-20T07:00:00",
                location = "Cape Town",
                distanceKm = 10,
                eventType = "Running"
            });

        response.EnsureSuccessStatusCode();

        var json =
            await response.Content.ReadFromJsonAsync<JsonElement>();

        return json.GetProperty("id").GetInt32();
    }

    [Fact]
    public async Task Organiser_CanAddRouteToOwnEvent()
    {
        var eventId = await CreateEventAsOrganiser();

        var response = await _client.PutAsJsonAsync(
            $"/api/events/{eventId}/route",
            new
            {
                startPoint = "Cape Town Stadium",
                finishPoint = "Cape Town Stadium",
                routeDescription = "Road route through Cape Town",
                elevationGainMetres = 85
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json =
            await response.Content.ReadFromJsonAsync<JsonElement>();

        Assert.Equal(
            "Cape Town Stadium",
            json.GetProperty("startPoint").GetString());

        Assert.Equal(
            85,
            json.GetProperty("elevationGainMetres").GetInt32());
    }

    [Fact]
    public async Task Participant_CannotUpdateRoute()
    {
        var eventId = await CreateEventAsOrganiser();

        var participantToken =
            await RegisterAndGetToken("Participant");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                participantToken);

        var response = await _client.PutAsJsonAsync(
            $"/api/events/{eventId}/route",
            new
            {
                startPoint = "Start",
                finishPoint = "Finish",
                routeDescription = "Participant should not change this",
                elevationGainMetres = 50
            });

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedUser_CanViewRoute()
    {
        var eventId = await CreateEventAsOrganiser();

        var putResponse = await _client.PutAsJsonAsync(
            $"/api/events/{eventId}/route",
            new
            {
                startPoint = "Start Line",
                finishPoint = "Finish Line",
                routeDescription = "Test route",
                elevationGainMetres = 100
            });

        putResponse.EnsureSuccessStatusCode();

        var participantToken =
            await RegisterAndGetToken("Participant");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                participantToken);

        var response = await _client.GetAsync(
            $"/api/events/{eventId}/route");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}