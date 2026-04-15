using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PlayMaker.Api;
using PlayMaker.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayMaker.Models.LiveScoreModel;
using PlayMaker.Models.Top10;

public class PollBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5); // anket kontrolü 5 dakikada bir

    private DateTime _lastCleanupDate = DateTime.MinValue; // silme işlemi için takip

    public PollBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var livescore = scope.ServiceProvider.GetRequiredService<LiveScoreServices>();
                var pollService = scope.ServiceProvider.GetRequiredService<PollService>();

                try
                {
                    // 5 dakikada bir anket oluştur
                    var liveData = await livescore.GetTeamsAsync();
                    var matchData = JsonConvert.DeserializeObject<MatchData>(liveData);
                    await pollService.CreatePollsIfNotExistAsync(matchData);

                    //Günde sadece 1 kere anket temizle
                    if (_lastCleanupDate.Date < DateTime.Now.Date && DateTime.Now.Hour >= 3)
                    {
                        await pollService.DeletePoll();
                        _lastCleanupDate = DateTime.Now.Date;
                        Console.WriteLine($"[PollCleaner] {DateTime.Now}: Eski anketler silindi.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PollBackgroundService] Hata: {ex.Message}");
                }
            }

            await Task.Delay(_interval, stoppingToken); // 5 dk bekle
        }
    }
}
