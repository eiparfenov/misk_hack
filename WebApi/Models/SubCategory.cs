namespace WebApi.Models;

public class SubCategory
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    
    public Category? Category { get; set; }
    public Guid CategoryId { get; set; }
    public List<Question>? Questions { get; set; }
}