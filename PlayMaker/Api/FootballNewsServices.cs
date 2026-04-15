using Microsoft.Extensions.Configuration;

namespace PlayMaker.Api
{
    public class FootballNewsServices
    {
        private readonly HttpClient _httpClient;
        private const string API_URL = "https://free-api-live-football-data.p.rapidapi.com/football-get-trendingnews";

        public FootballNewsServices(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-host", configuration["FootballNewsApi:Host"] ?? "free-api-live-football-data.p.rapidapi.com");
            _httpClient.DefaultRequestHeaders.Add("x-rapidapi-key",  configuration["FootballNewsApi:Key"] ?? "");
        }

        public async Task<string> GetNewsAsync()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(API_URL);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"API Hatası: {response.StatusCode}");
                    return null;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                return jsonResponse;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return null;
            }
        }
    }
}
