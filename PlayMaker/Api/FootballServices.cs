using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace PlayMaker.Api
{
    public class FootballService
    {
        private readonly HttpClient _httpClient;
        private const string API_URL = "https://api.collectapi.com/football/league?league="; 
        private readonly string _apiKey;
        private readonly IMemoryCache _memoryCache;

        public FootballService(HttpClient httpClient, IMemoryCache memoryCache, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _apiKey = configuration["CollectApi:Key"] ?? "";
        }

        public async Task<string?> GetLeaguesAsync(string url)
        {
            // Basit cache mekanizması
            string cacheKey = $"football_leagues_{url}";
            if (_memoryCache.TryGetValue(cacheKey, out string? cachedResult))
            {
                return cachedResult;
            }

            // Url kontrolü ve API çağrısı
            string apiurl = $"{API_URL}{url}";
            var request = new HttpRequestMessage(HttpMethod.Get, apiurl);
            // CollectAPI header contains ":" so add without strict validation
            request.Headers.TryAddWithoutValidation("authorization", _apiKey);

            try
            {
                Console.WriteLine($"[FootballService] Calling API: {apiurl}");
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                Console.WriteLine($"[FootballService] Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[FootballService] Response length: {result?.Length}, First 200 chars: {result?.Substring(0, Math.Min(200, result?.Length ?? 0))}");
                    
                    // Basit bir "success" check
                    if (!string.IsNullOrEmpty(result) && (result.Contains("\"success\": false") || result.Contains("\"success\":false")))
                    {
                        Console.WriteLine("[FootballService] API returned success:false");
                        return null; // API success false döndü
                    }

                    // Cache'e at (10 dk)
                    _memoryCache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
                    return result;
                }
                else
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"[FootballService] HTTP Error: {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FootballService] Exception: {ex.Message}");
                return null;
            }
        }

        // Bu metod artık kullanılmıyor ama eski kodda referans varsa diye stub olarak bırakıyorum
        // Ya da tamamen kaldırıyorum çünkü eski kod sadece GetLeaguesAsync kullanıyordu string dönen.
        // Ancak interface gereği vs. varsa diye dikkatli olmalıyım. 
        // Kullanıcının istediği "yarım saat önceki" kodda bu class string dönüyordu.
        
        // Yeni eklenen metodu siliyorum: GetStandingsAsync 
    }
}
