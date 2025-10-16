namespace WebApi.Models;

public class Message
{
    public required Guid Id { get; set; }
    public required string Question { get; set; }
    
    public required DateTimeOffset ReceiveUtc { get; set; }
    public Guid? SelectedAnswerId { get; set; }
    public Question? SelectedAnswer { get; set; }
    
    public string? EditedAnswer { get; set; }
    public DateTimeOffset AnsweredUtc { get; set; }
}