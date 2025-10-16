namespace WebApi.Models;

public class Category
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public List<SubCategory>? SubCategories { get; set; }
}