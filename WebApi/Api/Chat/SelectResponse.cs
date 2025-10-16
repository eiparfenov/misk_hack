using System.Text.Json;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace WebApi.Api.Chat;

public class SelectResponse(ApplicationDbContext db): Ep.Req<SelectResponse.RequestDto>.Res<Ok>
{
    public override void Configure()
    {
        Post("chat/select");
        AllowAnonymous();
    }

    public async override Task<Ok> ExecuteAsync(RequestDto req, CancellationToken ct)
    {
        Console.WriteLine(JsonSerializer.Serialize(req, new  JsonSerializerOptions() { WriteIndented = true }));
        return TypedResults.Ok();
    }

    public class RequestDto
    {
        public required Guid MessageId { get; set; }
        public required Guid? SelectedCategoryId { get; set; }
        public required Guid? SelectedSubCategory { get; set; }
        public required Guid? SelectedQuestionId { get; set; }
        public required string? EditedAnswerText { get; set; }
    }
}