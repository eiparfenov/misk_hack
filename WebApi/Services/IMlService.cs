namespace WebApi.Services;

public interface IMlService
{
    Task<ICollection<NluRecommendationView>> GetCategories(string message, CancellationToken cancellationToken);
    public record NluRecommendationView(
        double Accuracy,
        string Id,
        string Category,
        string SubCategory,
        string Question,
        string Priority,
        string Audience,
        string Answer
    );
}

public class MockMlService : IMlService
{
    public Task<ICollection<IMlService.NluRecommendationView>> GetCategories(string message,
        CancellationToken cancellationToke) => Task.FromResult<ICollection<IMlService.NluRecommendationView>>(
    [
        new IMlService.NluRecommendationView(
            0.9,
            Guid.NewGuid().ToString(),
            "Новые клиенты",
            "Регистрация и онбординг",
            "Регистрация через МСИ (Межбанковская система идентификации)",
            "высокий",
            "новые клиенты",
            "МСИ позволяет пройти идентификацию онлайн, используя данные других банков, где вы уже являетесь клиентом. Это упрощает процедуру регистрации и делает её быстрой и безопасной."
        )
    ]);
}
