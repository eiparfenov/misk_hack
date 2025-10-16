using FastEndpoints;

namespace WebApi.Api.Chat;

public class History: Ep.NoReq.Res<History.ResponseDto>
{
    public override void Configure()
    {
        Get("user/history");
        AllowAnonymous();
    }

    public override Task<ResponseDto> ExecuteAsync(CancellationToken ct)
    {
        return Task.FromResult(new ResponseDto
        {
            Messages = [
                new ResponseDto.MessageDto
                {
                    Date = "Тестовые данные",
                    Message = "Тестовые данные",
                    Category = "Тестовые данные",
                    SubCategory = "Тестовые данные",
                    Question = "Тестовые данные",
                    Answer = "Тестовые данные",
                    Priority = "Тестовые данные",
                    Audience = "Тестовые данные",
                }
            ]
        });
    }

    public class ResponseDto
    {
        public ICollection<MessageDto>? Messages { get; set; }
        public class MessageDto
        {
            public required string Date { get; set; }
            public required string Message { get; set; }
            public required string Category { get; set; }
            public required string SubCategory { get; set; }
            public required string Question { get; set; }
            public required string Answer { get; set; }
            public required string Priority { get; set; }
            public required string Audience { get; set; }
        }
    }
}