using PlayMaker.Data;
using PlayMaker.Entity;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PlayMaker.Models.LiveScoreModel;
using Microsoft.Extensions.FileSystemGlobbing;

public class PollService
{
    private readonly PlaymakerContext _context;

    public PollService(PlaymakerContext context)
    {
        _context = context;
    }

    public async Task CreatePollsIfNotExistAsync(MatchData matchData)
    {
        foreach (var stage in matchData.Stages)
        {
            foreach (var match in stage.Events)
            {
                var matchId = $"{match.T1[0].Nm} vs {match.T2[0].Nm}";

                // Eğer bu maç için zaten bir anket oluşturulmuşsa, geç
                var exists = await _context.Polls.AnyAsync(p => p.MatchId == matchId);
                if (exists) continue;

                // Yeni poll oluştur
                var poll = new Poll
                {
                    MatchId = matchId,
                    Title = $"{match.T1[0].Nm} vs {match.T2[0].Nm}",
                    Description = "Canlı maç tahmin anketi",
                    Time = DateTime.UtcNow,
                };

                _context.Polls.Add(poll);
            }
        }

        await _context.SaveChangesAsync();
    }


    public async Task DeletePoll()
    {
        var thresholdDate = DateTime.UtcNow.AddDays(-5);

        var oldPolls = await _context.Polls.Where(m => m.Time <thresholdDate).ToListAsync();

        if (oldPolls.Any())
        {
            _context.Polls.RemoveRange(oldPolls);
            await _context.SaveChangesAsync();
        }
    }


}
