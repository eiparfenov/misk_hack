using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApi;
using WebApi.Services;
using WebApi.Services.Initial;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFastEndpoints();
builder.Services.AddCors();
builder.Services.AddScoped<IMlService, MockMlService>();
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
{
    opts.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"),config => config.UseVector());
    opts.UseSnakeCaseNamingConvention();
});
builder.Services.AddHostedService<MigrateDb<ApplicationDbContext>>();
builder.Services.AddHostedService<FillEmbedings>();

builder.Services.AddHttpClient<IEmbedingsService, EmbeddingService>(opts =>
{
    opts.BaseAddress = new Uri(builder.Configuration.GetConnectionString("MlService")!);
});
var app = builder.Build();
app.UseCors(opts => opts.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
app.MapFastEndpoints(opts =>
{
    opts.Endpoints.RoutePrefix = "api";
});
app.MapGet("test", async ([FromServices] IEmbedingsService service, [Microsoft.AspNetCore.Mvc.FromQuery] string text) =>
{
    await service.GetTopQuestions(text);
});
app.Run();
// Как получить кмс по доте