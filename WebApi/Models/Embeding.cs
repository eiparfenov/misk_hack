using Pgvector;

namespace WebApi.Models;

public class Embeding
{
    public required Guid Id { get; set; }

    public required int Version { get; set; }
    public required Vector Vector { get; set; }

    public required Guid QuestionId { get; set; }
    public Question? Question { get; set; }
}