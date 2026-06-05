var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "ok",
    app = "MyWebApp",
    time = DateTimeOffset.UtcNow
}));

app.Run();
