namespace WebApi.Models;

public class Question
{
    public required Guid Id { get; set; }
    public required string Example { get; set; }
    public required string Priority { get; set; }
    public required string Audience { get; set; }
    public required string Answer { get; set; }

    public SubCategory? SubCategory { get; set; }
    public required Guid SubCategoryId { get; set; }
}