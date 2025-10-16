using Microsoft.EntityFrameworkCore;

namespace WebApi.Services.Initial;

public class MigrateDb<TDbContext>(IServiceScopeFactory serviceScopeFactory): BackgroundService where TDbContext : DbContext
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();
        await dbContext.Database.MigrateAsync(stoppingToken);
    }
}