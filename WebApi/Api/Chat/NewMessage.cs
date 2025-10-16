using FastEndpoints;
using Mapster;
using WebApi.Services;

namespace WebApi.Api.Chat;

public class NewMessage(IEmbedingsService mlService): Ep.Req<NewMessage.RequestDto>.Res<NewMessage.ResponseDto>
{
    public override void Configure()
    {
        Post("chat/newMessage");
        AllowAnonymous();
    }

    public override async Task<ResponseDto> ExecuteAsync(RequestDto req, CancellationToken ct)
    {
        var mlResponse = await mlService.GetTopQuestions(req.Message, cancellationToken: ct);
        return new ResponseDto()
        {
            Id = Guid.NewGuid().ToString(),
            NluRecommendations = [..(mlResponse ?? []).Select(m => m.Adapt<ResponseDto.NluRecommendationDto>())]
        };
    }

    public class RequestDto
    {
        public required string Message { get; set; }
    }

    public class ResponseDto
    {
        public required string Id { get; set; }
        public required ICollection<NluRecommendationDto> NluRecommendations { get; set; }
        public class NluRecommendationDto
        {
            public required double Accuracy { get; set; }
            public required string Id { get; set; }
            public required string Category { get; set; }
            public required string SubCategory { get; set; }
            public required string Question { get; set; }
            public required string Answer { get; set; }
            public required string Priority { get; set; }
            public required string Audience { get; set; }
        }
    }
}