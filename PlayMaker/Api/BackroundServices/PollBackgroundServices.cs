using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// Previously polled LiveScore every 5 minutes (quota killer).
/// LiveScore subscription is unavailable; keep hosted registration but do not call external APIs.
/// </summary>
public class PollBackgroundService : BackgroundService
{
    private readonly ILogger<PollBackgroundService> _logger;

    public PollBackgroundService(ILogger<PollBackgroundService> logger)
    {
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PollBackgroundService idle: LiveScore polling disabled to protect API quota.");
        return Task.CompletedTask;
    }
}
