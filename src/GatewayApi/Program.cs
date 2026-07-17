using Microsoft.EntityFrameworkCore;
using PearlMetric.GatewayApi.Data; // 1. Added this to let Program see PearlMetricDb

var builder = WebApplication.CreateBuilder(args);

// 2. Updated the database name in the connection string to match the new identity
var connectionString = "Host=localhost;Port=5432;Database=pearlmetric_dev;Username=polyadmin;Password=PolySecurePassword2026!";

builder.Services.AddDbContext<PearlMetricDb>(options =>
    options.UseNpgsql(connectionString));

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/health", () => Results.Ok(new
{
    Status = "OK",
    Timestamp = DateTime.UtcNow
}));

app.MapGet("/", () => Results.NotFound(new 
{
    status = "Nice One"
}));

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}