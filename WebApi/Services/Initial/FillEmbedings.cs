using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Services.Initial;

public class FillEmbedings(IServiceScopeFactory serviceScopeFactory): BackgroundService
{
    public const int CurrentEmbedingsVersion = 2;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var embedingService = scope.ServiceProvider.GetRequiredService<IEmbedingsService>();
        var existedEmbedings = await db.Embedings
            .Where(e => e.Version == CurrentEmbedingsVersion)
            .Select(e => e.QuestionId)
            .ToArrayAsync(stoppingToken);
        var questionsToFill = await db.Questions
            .Where(q => !existedEmbedings.Contains(q.Id))
            .Select(q => new { q.Id, q.Example })
            .ToArrayAsync(stoppingToken);
        List<Embeding> embeds = [];
        foreach (var q in questionsToFill)
        {
            var embed = await embedingService.GetEmbedings(q.Example);
            if (embed is null)
            {
                Console.WriteLine($"No embeds for {q.Example}");
                continue;
            }
            
            embeds.Add(new Embeding()
            {
                Id = Guid.NewGuid(),
                QuestionId = q.Id,
                Version = CurrentEmbedingsVersion,
                Vector = new Pgvector.Vector(embed)
            });
        }
        db.AddRange(embeds);
        await db.SaveChangesAsync(stoppingToken);
    }
}