using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Api.Chat;

public class QuestionTree(ApplicationDbContext db): Ep.NoReq.Res<QuestionTree.ResponseDto>
{
    public override void Configure()
    {
        Get("chat/questions");
        AllowAnonymous();
    }

    public override async Task<ResponseDto> ExecuteAsync(CancellationToken ct)
    {
        var tree = await db.Categories
            .Include(c => c.SubCategories!)
            .ThenInclude(sc => sc.Questions)
            .ToArrayAsync(ct);
        return new ResponseDto
        {
            Categories =
            [
                ..tree.Select(c => new ResponseDto.CategoryDto
                {
                    Id = c.Id.ToString(),
                    Title = c.Title,
                    SubCategories =
                    [
                        .. c.SubCategories!.Select(sc => new ResponseDto.SubCategoryDto()
                        {
                            Id = sc.Id.ToString(),
                            Title = sc.Title,
                            Questions =
                            [
                                ..sc.Questions!.Select(q => new ResponseDto.QuestionDto()
                                {
                                    Id = q.Id.ToString(),
                                    Title = q.Example,
                                    Priority = q.Priority,
                                    Audience = q.Audience,
                                })
                            ]
                        })
                    ]
                })
            ]
        };
    }

    public class ResponseDto
    {
        public required ICollection<CategoryDto> Categories { get; set; }
        public class CategoryDto
        {
            public required string Id { get; set; }
            public required string Title { get; set; }
            public required ICollection<SubCategoryDto> SubCategories { get; set; }
        }
        public class SubCategoryDto
        {
            public required string Id { get; set; }
            public required string Title { get; set; }
            public required ICollection<QuestionDto> Questions { get; set; }
        }
        public class QuestionDto
        {
            public required string Id { get; set; }
            public required string Title { get; set; }
            public required string Audience { get; set; }
            public required string Priority { get; set; }
        }
    }
}