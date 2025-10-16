using FastEndpoints;
using WebApi.Services;

namespace WebApi.Api.Chat;

public class GetMlAnswer(IEmbedingsService embedingsService): Ep.Req<GetMlAnswer.RequestDto>.Res<GetMlAnswer.ResponseDto>
{
    public override void Configure()
    {
        Post("chat/request_answer_completion");
        AllowAnonymous();
    }

    public override async Task<ResponseDto> ExecuteAsync(RequestDto req, CancellationToken ct)
    {
        var text = await embedingsService.GetMlText(req.Question,  req.Answer, ct);
        return new ResponseDto { MlAnswer = text };
    }

    public class ResponseDto
    {
        public required string MlAnswer { get; set; }
    }

    public class RequestDto
    {
        public required string Question { get; set; }
        public required string Answer { get; set; }
    }
}