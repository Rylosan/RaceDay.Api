
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

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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

    [Fact]
    public async Task Participant_CannotUploadEventImage()
    {
        // Create an event as an organiser first.
        var organiserToken = await RegisterAndGetToken("Organiser");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", organiserToken);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/Events",
            new
            {
                name = "Upload Permission Test",
                description = "Testing image upload permissions",
                eventDate = "2026-12-01T07:00:00",
                location = "Cape Town",
                distanceKm = 10,
                eventType = "Running"
            });

        createResponse.EnsureSuccessStatusCode();

        var eventJson =
            await createResponse.Content.ReadFromJsonAsync<JsonElement>();

        var eventId = eventJson.GetProperty("id").GetInt32();

        // Attempt to upload as a participant.
        var participantToken =
            await RegisterAndGetToken("Participant");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", participantToken);

        using var content = new MultipartFormDataContent();

        var response = await _client.PostAsync(
            $"/api/Events/{eventId}/image",
            content);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AuthenticatedUser_GetsNotFoundWhenEventHasNoImage()
    {
        // Create an event as an organiser.
        var organiserToken = await RegisterAndGetToken("Organiser");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", organiserToken);

        var createResponse = await _client.PostAsJsonAsync(
            "/api/Events",
            new
            {
                name = "Image Test Event",
                description = "Testing an event without an image",
                eventDate = "2026-12-01T07:00:00",
                location = "Cape Town",
                distanceKm = 10,
                eventType = "Running"
            });

        createResponse.EnsureSuccessStatusCode();

        var eventJson =
            await createResponse.Content.ReadFromJsonAsync<JsonElement>();

        var eventId = eventJson.GetProperty("id").GetInt32();

        // Request the image as an authenticated participant.
        var participantToken =
            await RegisterAndGetToken("Participant");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", participantToken);

        var response = await _client.GetAsync(
            $"/api/Events/{eventId}/image");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
