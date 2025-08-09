using System.Net;
using System.Runtime.CompilerServices;
using FivetranClient.Fetchers;
using FivetranClient.Infrastructure;
using FivetranClient.Models;

namespace FivetranClient;

public record DataSchemas(Dictionary<string, Schema?> Schemas);
public record NonPaginatedRoot<T>(T? Data);
public record PaginatedRoot<T>(Data<T> Data);

public class RestApiManager(HttpRequestHandler requestHandler) : IDisposable
{
    private readonly PaginatedFetcher _paginatedFetcher = new(requestHandler);
    private readonly NonPaginatedFetcher _nonPaginatedFetcher = new(requestHandler);
    private readonly HttpClient? _createdClient;

    private const string _apiBaseUrl = "https://api.fivetran.com/v1/";
    private const string _groupsEndpoint = "groups";
    private const string _connectorsEndpoint = "connectors";
    private const string _schemasEndpoint = "schemas";

    public static readonly Uri ApiBaseUrl = new(_apiBaseUrl);

    public RestApiManager(string apiKey, string apiSecret, TimeSpan timeout) : this(ApiBaseUrl, apiKey, apiSecret, timeout)
    {

    }

    public RestApiManager(Uri baseUrl, string apiKey, string apiSecret, TimeSpan timeout) : this(new FivetranHttpClient(baseUrl, apiKey, apiSecret, timeout), true)
    {

    }
    private RestApiManager(HttpClient client, bool _) : this(new HttpRequestHandler(client)) => _createdClient = client;

    public RestApiManager(HttpClient client) : this(new HttpRequestHandler(client))
    {

    }

    public async IAsyncEnumerable<Group> GetGroupsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var item in _paginatedFetcher.FetchAllItemsAsync<Group>(_groupsEndpoint, cancellationToken))
        {
            yield return item;
        }
    }

    public async IAsyncEnumerable<Connector> GetConnectorsAsync(string groupId, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var endpointPath = $"{_groupsEndpoint}/{WebUtility.UrlEncode(groupId)}/{_connectorsEndpoint}";

        await foreach (var item in _paginatedFetcher.FetchAllItemsAsync<Connector>(endpointPath, cancellationToken))
        {
            yield return item;
        }
    }

    public async Task<DataSchemas?> GetConnectorSchemasAsync(string connectorId, CancellationToken cancellationToken)
    {
        var endpointPath = $"{_connectorsEndpoint}/{WebUtility.UrlEncode(connectorId)}/{_schemasEndpoint}";

        return await _nonPaginatedFetcher.FetchAsync<DataSchemas>(endpointPath, cancellationToken);
    }

    public void Dispose()
    {
        _createdClient?.Dispose();

        GC.SuppressFinalize(this);
    }
}