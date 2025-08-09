using System.Net.Http.Headers;
using System.Text;

namespace FivetranClient.Infrastructure;

public class FivetranHttpClient : HttpClient
{
    private const string _userAgent = "aseduigbn";
    private const string _authorizationScheme = "Basic";
    private const string _jsonMediaType = "application/json";
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(40);

    public FivetranHttpClient(Uri baseAddress, string apiKey, string apiSecret, TimeSpan timeout) : base()
    {
        if (baseAddress == null)
        {
            throw new ArgumentNullException(nameof(baseAddress));
        }
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentNullException(nameof(apiKey));
        }
        if (string.IsNullOrWhiteSpace(apiSecret))
        {
            throw new ArgumentNullException(nameof(apiSecret));
        }
        if (timeout.Ticks <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be a positive value.");
        }

        BaseAddress = baseAddress;
        DefaultRequestHeaders.Clear();
        DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_authorizationScheme, CalculateToken(apiKey, apiSecret));
        DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_jsonMediaType));
        DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
        Timeout = timeout;
    }

    public FivetranHttpClient(Uri baseAddress, string apiKey, string apiSecret) : this(baseAddress, apiKey, apiSecret, _defaultTimeout)
    {

    }

    private static string CalculateToken(string apiKey, string apiSecret)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentNullException(nameof(apiKey));
        }

        if (string.IsNullOrWhiteSpace(apiSecret))
        {
            throw new ArgumentNullException(nameof(apiSecret));
        }

        var tokenBytes = Encoding.ASCII.GetBytes($"{apiKey}:{apiSecret}");
        return Convert.ToBase64String(tokenBytes);
    }
}