using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace PlayMaker.Api
{
    public class LiveScoreServices
    {
        private readonly HttpClient _httpClient;
        private static readonly string apiUrl = "https://livescore6.p.rapidapi.com/matches/v2/list-live?Category=soccer&Timezone=-7";
        private readonly string _apiKey;
        private readonly string _apiHost;

        public LiveScoreServices(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiHost = configuration["LiveScoreApi:Host"] ?? "livescore6.p.rapidapi.com";
            _apiKey  = configuration["LiveScoreApi:Key"] ?? "";
        }

        public async Task<string> GetTeamsAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
                request.Headers.Add("x-rapidapi-host", _apiHost);
                request.Headers.Add("x-rapidapi-key", _apiKey);

                HttpResponseMessage response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    // Hata durumunda boş string veya null dönmek yerine hatayı fırlatabilirsiniz. 
                    // Ancak mevcut yapı string mesaj döndüğü için bunu koruyoruz ama logluyoruz.
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

