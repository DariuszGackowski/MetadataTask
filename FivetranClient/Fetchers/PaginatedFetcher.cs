using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using FivetranClient.Models;

namespace FivetranClient.Fetchers;

public sealed class PaginatedFetcher(HttpRequestHandler requestHandler) : BaseFetcher(requestHandler)
{
    private const int PageSize = 1000;

    public async IAsyncEnumerable<T> FetchAllItemsAsync<T>(string endpoint, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        string? nextCursor = null;

        while (true)
        {
            var currentPage = await FetchPageAsync<T>(endpoint, nextCursor, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            if (currentPage?.Data == null)
            {
                throw new InvalidOperationException($"API returned an invalid or null data structure for endpoint: {endpoint}.");
            }

            var items = currentPage.Data.Items;
            nextCursor = currentPage.Data.NextCursor;

            if (items == null)
            {
                throw new InvalidOperationException($"API returned a null list of items for endpoint: {endpoint}.");
            }

            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return item;
            }

            if (string.IsNullOrWhiteSpace(nextCursor))
            {
                yield break;
            }
        }
    }

    private async Task<PaginatedRoot<T>> FetchPageAsync<T>(string endpoint, string? cursor, CancellationToken cancellationToken)
    {
        var url = BuildUrl(endpoint, cursor);
        var response = await RequestHandler.GetAsync(url, cancellationToken);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        var result = JsonSerializer.Deserialize<PaginatedRoot<T>>(content, SerializerOptions);
        if (result == null)
        {
            throw new JsonException($"Failed to deserialize the response for endpoint: {endpoint}.");
        }
        return result;
    }

    private string BuildUrl(string endpoint, string? cursor)
    {
        var urlBuilder = new StringBuilder(endpoint);
        urlBuilder.Append($"?limit={PageSize}");

        if (!string.IsNullOrWhiteSpace(cursor))
        {
            urlBuilder.Append($"&cursor={WebUtility.UrlEncode(cursor)}");
        }
        return urlBuilder.ToString();
    }
}