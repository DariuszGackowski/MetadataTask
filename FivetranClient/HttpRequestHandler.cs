using System.Net;
using FivetranClient.Infrastructure;

public class HttpRequestHandler
{
    private readonly HttpClient _client;
    private readonly SemaphoreSlim _semaphore;
    private readonly object _lock = new();
    private DateTime _retryAfterTime = DateTime.UtcNow;
    private static readonly TtlDictionary<string, HttpResponseMessage> _responseCache = new();

    public HttpRequestHandler(HttpClient client, ushort maxConcurrentRequests = 10) : this(client, new SemaphoreSlim(maxConcurrentRequests, maxConcurrentRequests))
    {
    }

    private HttpRequestHandler(HttpClient client, SemaphoreSlim semaphore)
    {
        _client = client;
        _semaphore = semaphore;
    }

    public async Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken)
    {
        return await _responseCache.GetOrAddAsync(url, async () => await GetResponseAsync(url, cancellationToken), TimeSpan.FromMinutes(60));
    }

    private async Task<HttpResponseMessage> GetResponseAsync(string url, CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);

        try
        {
            TimeSpan timeToWait;
            lock (_lock)
            {
                timeToWait = _retryAfterTime - DateTime.UtcNow;
            }

            if (timeToWait > TimeSpan.Zero)
            {
                await Task.Delay(timeToWait, cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var response = await _client.GetAsync(new Uri(url, UriKind.RelativeOrAbsolute), cancellationToken);

            if (response.StatusCode is HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);

                lock (_lock)
                {
                    _retryAfterTime = DateTime.UtcNow.Add(retryAfter);
                }

                throw new HttpRequestException($"Rate limit exceeded. Retry after {retryAfter.TotalSeconds} seconds.");
            }

            response.EnsureSuccessStatusCode();
            return response;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}