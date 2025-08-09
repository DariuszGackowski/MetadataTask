using System.Text.Json;
using FivetranClient.Models;

namespace FivetranClient.Fetchers;

public sealed class NonPaginatedFetcher(HttpRequestHandler requestHandler) : BaseFetcher(requestHandler)
{
    public async Task<T?> FetchAsync<T>(string endpoint, CancellationToken cancellationToken) where T : class
    {
        var response = await RequestHandler.GetAsync(endpoint, cancellationToken);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrEmpty(content))
        {
            return default;
        }

        var root = JsonSerializer.Deserialize<NonPaginatedRoot<T>>(content, SerializerOptions);
        return root?.Data;
    }
}