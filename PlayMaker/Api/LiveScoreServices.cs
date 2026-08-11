using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PlayMaker.Api
{
    public class LiveScoreServices
    {
        private readonly HttpClient _httpClient;
        private static readonly string apiUrl = "https://livescore6.p.rapidapi.com/matches/v2/list-live?Category=soccer&Timezone=-7";
        private readonly string _apiKey;
        private readonly string _apiHost;
        private readonly ILogger<LiveScoreServices> _logger;

        public LiveScoreServices(HttpClient httpClient, IConfiguration configuration, ILogger<LiveScoreServices> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiHost = configuration["LiveScoreApi:Host"] ?? "livescore6.p.rapidapi.com";
            _apiKey = configuration["LiveScoreApi:Key"] ?? "";
        }

        /// <summary>
        /// Returns JSON body on success; null when unavailable (auth, quota, timeout, non-JSON).
        /// Does not throw — callers should show an empty/unavailable UI state.
        /// </summary>
        public async Task<string?> GetTeamsAsync()
        {
            if (string.IsNullOrWhiteSpace(_apiKey) || _apiKey.Contains("YOUR_", StringComparison.Ordinal))
            {
                _logger.LogInformation("LiveScore API key missing or placeholder; skipping request");
                return null;
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.TryAddWithoutValidation("x-rapidapi-host", _apiHost);
                request.Headers.TryAddWithoutValidation("x-rapidapi-key", _apiKey);

                using var response = await _httpClient.SendAsync(request);
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("LiveScore unavailable (HTTP {Status})", (int)response.StatusCode);
                    return null;
                }

                if (string.IsNullOrWhiteSpace(body) || body.TrimStart().StartsWith("<", StringComparison.Ordinal))
                    return null;

                // Reject plain-text error payloads that would break JSON deserialization.
                if (!body.TrimStart().StartsWith("{", StringComparison.Ordinal) &&
                    !body.TrimStart().StartsWith("[", StringComparison.Ordinal))
                    return null;

                return body;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "LiveScore request failed");
                return null;
            }
        }
    }
}
