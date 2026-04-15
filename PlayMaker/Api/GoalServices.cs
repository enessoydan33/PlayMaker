using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayMaker.Api
{
    public class GoalServices
    {

        private readonly HttpClient _httpClient;
        private const string API_URL = "https://api.collectapi.com/football/goalKings?league=";
        private readonly string _apiKey;
        private readonly IMemoryCache _memoryCache;
        
        public GoalServices(HttpClient httpClient, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _apiKey = configuration["CollectApi:Key"] ?? "";
        }

        public async Task<string?> GetLeaguesAsync(string key)
        {
            try
            {
                string cacheKey = $"goals_{key}";
                if (_memoryCache.TryGetValue(cacheKey, out string? cachedResult))
                {
                    return cachedResult;
                }

                string apiurl = $"{API_URL}{key}";
                Console.WriteLine($"[GoalServices] URL = {apiurl}");

                var request = new HttpRequestMessage(HttpMethod.Get, apiurl);
                // CollectAPI header contains ":" so add without strict validation
                request.Headers.TryAddWithoutValidation("authorization", _apiKey);

                HttpResponseMessage response = await _httpClient.SendAsync(request);
                string result = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[GoalServices] Status = {response.StatusCode}");
                
                // API başarısız olsa bile 200 dönebilir, içeriği kontrol et
                if (response.IsSuccessStatusCode)
                {
                    // JSON formati kontrolu
                    var trimmed = result.Trim();
                    if (!trimmed.StartsWith("{") && !trimmed.StartsWith("["))
                    {
                         System.Diagnostics.Debug.WriteLine($"GoalServices API Error: Response is not JSON. {result}");
                         return null;
                    }

                    // JSON içinde "success":false kontrolü yap
                    if (result.Contains("\"success\":false") || result.Contains("\"success\": false"))
                    {
                        System.Diagnostics.Debug.WriteLine($"GoalServices API Error: {result}");
                        return null;
                    }
                    
                    _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(15));
                    return result;
                }
                else
                {
                    Console.WriteLine($"[GoalServices] HTTP Error: {response.StatusCode} - {result}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GoalServices] Exception: {ex}");
                return null;
            }
        }


    }
}
