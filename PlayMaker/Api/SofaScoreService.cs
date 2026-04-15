using Microsoft.Extensions.Configuration;

namespace PlayMaker.Api
{
    public class SofaScoreService
    {
        private readonly HttpClient _httpClient;
        private const string API_URL = "https://api-football-v1.p.rapidapi.com/v3/leagues";
        private readonly string _apiKey;
        private readonly string _apiHost;

        public SofaScoreService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            _apiHost = configuration["SofaScoreApi:Host"] ?? "sofascore.p.rapidapi.com";
            _apiKey  = configuration["SofaScoreApi:Key"] ?? "";
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", _apiHost);
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key", _apiKey);
        }

        public async Task<string> GetTeamRankingsAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(API_URL);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return $"Hata! HTTP {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                return $"Bir hata oluştu: {ex.Message}";
            }
        }
    }
}
