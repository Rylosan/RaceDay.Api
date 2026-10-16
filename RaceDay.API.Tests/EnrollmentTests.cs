using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class EnrollmentTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EnrollmentTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> RegisterAndGetToken(string role)
    {
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

    private async Task<(int eventId, int categoryId)> CreateEventAndCategory()
    {
        var organiserToken =
            await RegisterAndGetToken("Organiser");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                organiserToken);

        var eventResponse = await _client.PostAsJsonAsync(
            "/api/Events",
            new
            {
                name = "Enrollment Test 10K",
                description = "Event used by automated tests",
                eventDate = "2026-12-10T07:00:00",
                location = "Cape Town",
                distanceKm = 10,
                eventType = "Running"
            });

        eventResponse.EnsureSuccessStatusCode();

        var eventJson =
            await eventResponse.Content.ReadFromJsonAsync<JsonElement>();

        var eventId = eventJson.GetProperty("id").GetInt32();

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/categories",
            new
            {
                name = "Open 10K",
                description = "Open test category"
            });

        categoryResponse.EnsureSuccessStatusCode();

        var categoryJson =
            await categoryResponse.Content.ReadFromJsonAsync<JsonElement>();

        var categoryId =
            categoryJson.GetProperty("id").GetInt32();

        return (eventId, categoryId);
    }

    [Fact]
    public async Task Participant_CanEnrollInEvent()
    {
        var (eventId, categoryId) =
            await CreateEventAndCategory();

        var participantToken =
            await RegisterAndGetToken("Participant");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                participantToken);

        var response = await _client.PostAsJsonAsync(
            "/api/Enrollments",
            new
            {
                eventId,
                categoryId
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Organiser_CannotEnrollInEvent()
    {
        var (eventId, categoryId) =
            await CreateEventAndCategory();

        var organiserToken =
            await RegisterAndGetToken("Organiser");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                organiserToken);

        var response = await _client.PostAsJsonAsync(
            "/api/Enrollments",
            new
            {
                eventId,
                categoryId
            });

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotEnrollTwiceInSameEvent()
    {
        var (eventId, categoryId) =
            await CreateEventAndCategory();

        var participantToken =
            await RegisterAndGetToken("Participant");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                participantToken);

        var enrollment = new
        {
            eventId,
            categoryId
        };

        var firstResponse =
            await _client.PostAsJsonAsync(
                "/api/Enrollments",
                enrollment);

        Assert.Equal(
            HttpStatusCode.OK,
            firstResponse.StatusCode);

        var secondResponse =
            await _client.PostAsJsonAsync(
                "/api/Enrollments",
                enrollment);

        Assert.Equal(
            HttpStatusCode.BadRequest,
            secondResponse.StatusCode);
    }
}