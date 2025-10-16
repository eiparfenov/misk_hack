using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using WebApi.Services.Initial;

namespace WebApi.Services;

public interface IEmbedingsService
{
    Task<float[]?> GetEmbedings(string text, CancellationToken cancellationToken = default);
    Task<ICollection<TopQuestionView>?> GetTopQuestions(string text, int count = 3, CancellationToken cancellationToken = default);
    Task<string> GetMlText(string question, string answer, CancellationToken cancellationToken = default);
    public record TopQuestionView(
        double Accuracy,
        Guid Id,
        string Category,
        string SubCategory,
        string Question,
        string Priority,
        string Audience,
        string Answer
        );
}

public class MockEmbedingsService(ApplicationDbContext db, HttpClient httpClient) : IEmbedingsService
{
    public async Task<float[]?> GetEmbedings(string text, CancellationToken cancellationToken = default)
    {
        var qs = QueryString.Create("text", text);
        var httpResponse = await httpClient.GetAsync("/embeds" + qs);
        if (!httpResponse.IsSuccessStatusCode) return null;
        var response = await httpResponse.Content.ReadFromJsonAsync<ResponseDto>();
        return response?.Embeding;
    }

    public async Task<ICollection<IEmbedingsService.TopQuestionView>?> GetTopQuestions(string text, int count = 3, CancellationToken cancellationToken = default)
    {
        var embeds = await GetEmbedings(text);
        if(embeds is null) return [];
        var embedVector = new Pgvector.Vector(embeds);
        var topQuestions = await db.Embedings
            .Where(e => e.Version == FillEmbedings.CurrentEmbedingsVersion)
            .Select(e => new
            {
                Distance = e.Vector.CosineDistance(embedVector),
                e.QuestionId,
                CategoryTitle = e.Question!.SubCategory!.Category!.Title,
                SubCategoryTitle = e.Question!.SubCategory!.Title,
                Question = e.Question.Example,
                Priority = e.Question!.Priority,
                Audience = e.Question!.Audience,
                Answer = e.Question!.Answer
            })
            .OrderBy(e => e.Distance)
            .Take(count)
            .ToArrayAsync();

        return
        [
            .. topQuestions.Select(e => new IEmbedingsService.TopQuestionView(
                .9,
                e.QuestionId,
                e.CategoryTitle,
                e.SubCategoryTitle,
                e.Question,
                e.Priority,
                e.Audience,
                e.Answer
            ))
        ];
    }

    public async Task<string> GetMlText(string question, string answer, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private class ResponseDto
    {
        public float[] Embeding { get; set; }
    }
}

public class EmbeddingService(ApplicationDbContext db, HttpClient httpClient) : IEmbedingsService
{
    public async Task<float[]?> GetEmbedings(string text, CancellationToken cancellationToken = default)
    {
        var httpCreateTaskResponse = await httpClient.PostAsJsonAsync("/request_hint", new{ Question = text}, cancellationToken);
        var createTaskResponse = await httpCreateTaskResponse.Content.ReadFromJsonAsync<CreateTaskResponse>(cancellationToken);
        
        float[]? embeds = null;
        var maxRetries = 10;
        do
        {
            Console.WriteLine(maxRetries);
            --maxRetries;
            await Task.Delay(TimeSpan.FromSeconds(.2), cancellationToken);
            var httpTaskStatusResponse = await httpClient.PostAsJsonAsync("/task_status", createTaskResponse, cancellationToken);
            var taskStatusResponse = await httpTaskStatusResponse.Content.ReadFromJsonAsync<TaskStatusResponse>(cancellationToken);
            embeds = taskStatusResponse?.Embedding;
        } while (embeds is null && maxRetries > 0);

        return embeds;
    }

    public async Task<ICollection<IEmbedingsService.TopQuestionView>?> GetTopQuestions(string text, int count = 3, CancellationToken cancellationToken = default)
    {
        var embeds = await GetEmbedings(text);
        Console.WriteLine(embeds is null);
        if(embeds is null) return [];
        var embedVector = new Pgvector.Vector(embeds);
        var topQuestions = await db.Embedings
            .Where(e => e.Version == FillEmbedings.CurrentEmbedingsVersion)
            .Select(e => new
            {
                Distance = e.Vector.CosineDistance(embedVector),
                e.QuestionId,
                CategoryTitle = e.Question!.SubCategory!.Category!.Title,
                SubCategoryTitle = e.Question!.SubCategory!.Title,
                Question = e.Question.Example,
                Priority = e.Question!.Priority,
                Audience = e.Question!.Audience,
                Answer = e.Question!.Answer
            })
            .OrderBy(e => e.Distance)
            .Take(count)
            .ToArrayAsync(cancellationToken);

        return
        [
            .. topQuestions.Select(e => new IEmbedingsService.TopQuestionView(
                (1 - e.Distance) * 100,
                e.QuestionId,
                e.CategoryTitle,
                e.SubCategoryTitle,
                e.Question,
                e.Priority,
                e.Audience,
                e.Answer
            ))
        ];
    }

    public async Task<string> GetMlText(string question, string answer, CancellationToken cancellationToken = default)
    {
        var httpCreateTaskResponse = await httpClient.PostAsJsonAsync("/request_answer_completion", new CreateTextTaskRequest()
        {
            Answer = answer,
            Question = question
        }, cancellationToken);
        var createTaskResponse = await httpCreateTaskResponse.Content.ReadFromJsonAsync<CreateTaskResponse>(cancellationToken);
        
        string? embeds = null;
        var maxRetries = 12;
        do
        {
            --maxRetries;
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            var httpTaskStatusResponse = await httpClient.PostAsJsonAsync("/task_status", createTaskResponse, cancellationToken);
            var taskStatusResponse = await httpTaskStatusResponse.Content.ReadFromJsonAsync<TextTaskStatusResponse>(cancellationToken);
            embeds = taskStatusResponse?.Text;
        } while (embeds is null && maxRetries > 0);

        Console.WriteLine($"----->>>>> {maxRetries}");
        return embeds ?? "Произошла ошибка при генерации, воспользуйтесь шаблонным ответом";
    }

    private class CreateTextTaskRequest
    {
        [JsonPropertyName("question")] public required string Question { get; set; }
        [JsonPropertyName("answer")] public required string Answer { get; set; }
    }

    private class TextTaskStatusResponse
    {
        [JsonPropertyName("completed_answer")] public string? Text { get; set; }
    }
    private class CreateTaskResponse
    {
        [JsonPropertyName("task_id")]
        public required string TaskId { get; set; }
    }
    
    private class TaskStatusResponse
    {
        [JsonPropertyName("embedding")] public float[]? Embedding { get; set; }
        [JsonPropertyName("Основная категория")] public string? Category { get; set; }
        [JsonPropertyName("Подкатегория")] public string? SubCategory { get; set; }
        [JsonPropertyName("Целевая аудитория")] public string? Audience { get; set; }
    }
}