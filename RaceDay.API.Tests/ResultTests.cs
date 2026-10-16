using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace RaceDay.API.Tests;

public class ResultTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ResultTests(CustomWebApplicationFactory factory)
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

    private async Task<int> CreateEnrollment()
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
                name = "Result Test 10K",
                description = "Event used for result testing",
                eventDate = "2026-12-15T07:00:00",
                location = "Cape Town",
                distanceKm = 10,
                eventType = "Running"
            });

        eventResponse.EnsureSuccessStatusCode();

        var eventJson =
            await eventResponse.Content.ReadFromJsonAsync<JsonElement>();

        var eventId =
            eventJson.GetProperty("id").GetInt32();

        var categoryResponse = await _client.PostAsJsonAsync(
            $"/api/events/{eventId}/categories",
            new
            {
                name = "Open",
                description = "Open category"
            });

        categoryResponse.EnsureSuccessStatusCode();

        var categoryJson =
            await categoryResponse.Content.ReadFromJsonAsync<JsonElement>();

        var categoryId =
            categoryJson.GetProperty("id").GetInt32();

        var participantToken =
            await RegisterAndGetToken("Participant");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                participantToken);

        var enrollmentResponse =
            await _client.PostAsJsonAsync(
                "/api/Enrollments",
                new
                {
                    eventId,
                    categoryId
                });

        enrollmentResponse.EnsureSuccessStatusCode();

        var enrollmentJson =
            await enrollmentResponse.Content
                .ReadFromJsonAsync<JsonElement>();

        return enrollmentJson
            .GetProperty("id")
            .GetInt32();
    }

    [Fact]
    public async Task DifferentOrganiser_CannotRecordResult()
    {
        var enrollmentId = await CreateEnrollment();

        var organiserToken =
            await RegisterAndGetToken("Organiser");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                organiserToken);

        var response = await _client.PostAsJsonAsync(
            "/api/Results",
            new
            {
                enrollmentId,
                finishTime = "00:50:00",
                position = 1
            });

        // This organiser did not create the event,
        // so ownership protection should prevent access.
        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task Participant_CannotRecordResult()
    {
        var enrollmentId = await CreateEnrollment();

        var participantToken =
            await RegisterAndGetToken("Participant");

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                participantToken);

        var response = await _client.PostAsJsonAsync(
            "/api/Results",
            new
            {
                enrollmentId,
                finishTime = "00:50:00",
                position = 1
            });

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }
}