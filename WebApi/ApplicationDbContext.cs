using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi;

public class ApplicationDbContext: DbContext
{
    public required DbSet<Category> Categories { get; set; }
    public required DbSet<SubCategory> SubCategories { get; set; }
    public required DbSet<Question> Questions { get; set; }
    public required DbSet<Embeding> Embedings { get; set; }

    public ApplicationDbContext(DbContextOptions options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("category");
        });
        modelBuilder.Entity<SubCategory>(entity =>
        {
            entity.ToTable("sub_category");
        });
        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("question");
        });
        modelBuilder.Entity<Embeding>(entity =>
        {
            entity.ToTable("embeding");
            entity.Property(e => e.Vector).HasColumnType("vector(1024)");
        });
    }
}