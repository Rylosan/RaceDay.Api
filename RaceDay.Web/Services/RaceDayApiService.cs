
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace RaceDay.Web.Services;

public class RaceDayApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RaceDayApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CreateClient(string? token = null)
    {
        var client = _httpClientFactory.CreateClient("RaceDayAPI");

        if (!string.IsNullOrWhiteSpace(token))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        return client;
    }

    // GET request
    public async Task<T?> GetAsync<T>(
        string endpoint,
        string? token = null)
    {
        var client = CreateClient(token);

        return await client.GetFromJsonAsync<T>(endpoint);
    }

    // GET image from API
    public async Task<(byte[] Content, string ContentType)?> GetImageAsync(
        string endpoint,
        string? token = null)
    {
        var client = CreateClient(token);

        using var response = await client.GetAsync(endpoint);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsByteArrayAsync();

        var contentType = response.Content.Headers.ContentType?.MediaType
            ?? "application/octet-stream";

        return (content, contentType);
    }

    // POST request
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        string? token = null)
    {
        var client = CreateClient(token);

        var response = await client.PostAsJsonAsync(endpoint, data);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    // PUT request
    public async Task<TResponse?> PutAsync<TRequest, TResponse>(
        string endpoint,
        TRequest data,
        string? token = null)
    {
        var client = CreateClient(token);

        var response = await client.PutAsJsonAsync(endpoint, data);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    // DELETE request
    public async Task DeleteAsync(
        string endpoint,
        string? token = null)
    {
        var client = CreateClient(token);

        var response = await client.DeleteAsync(endpoint);

        response.EnsureSuccessStatusCode();
    }
}
