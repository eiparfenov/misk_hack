using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Api.Chat;

public class GetAnswer(ApplicationDbContext db) : Ep
    .Req<GetAnswer.RequestDto>
    .Res<
        Results<
            Ok<GetAnswer.ResponseDto>,
            NotFound
        >
    >
{
    public override void Configure()
    {
        Get("chat/answer/{questionId::guid}");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<ResponseDto>, NotFound>> ExecuteAsync(RequestDto req, CancellationToken ct)
    {
        var question = await db.Questions.SingleOrDefaultAsync(q => q.Id == req.QuestionId, ct);
        if (question is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new ResponseDto()
        {
            Text = question.Answer,
        });
    }


    public class RequestDto
    {
        [FromRoute] public required Guid QuestionId { get; set; }
    }

    public class ResponseDto
    {
        public required string Text { get; set; }
    }
}