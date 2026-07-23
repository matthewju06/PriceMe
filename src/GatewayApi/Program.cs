using Microsoft.EntityFrameworkCore;
using PearlMetric.GatewayApi.Configuration;
using PearlMetric.GatewayApi.Data;
using PearlMetric.GatewayApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PearlMetric")
    ?? throw new InvalidOperationException(
        "Connection string 'PearlMetric' is required. Configure it with user secrets or the ConnectionStrings__PearlMetric environment variable.");

builder.Services.AddDbContext<PearlMetricDb>(options =>
    options.UseNpgsql(connectionString));

builder.Services
    .AddOptions<CvWorkerOptions>()
    .Bind(builder.Configuration.GetSection(CvWorkerOptions.SectionName))
    .Validate(
        options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps),
        "CvWorker:BaseUrl must be an absolute HTTP or HTTPS URL.")
    .Validate(
        options => options.TimeoutSeconds is > 0 and <= 300,
        "CvWorker:TimeoutSeconds must be between 1 and 300 seconds.")
    .ValidateOnStart();

builder.Services
    .AddOptions<ImageStorageOptions>()
    .Bind(builder.Configuration.GetSection(ImageStorageOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.RootPath),
        "ImageStorage:RootPath is required.")
    .ValidateOnStart();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new
{
    Status = "OK",
    Timestamp = DateTime.UtcNow
}))
.WithName("Health");

app.MapApiContractEndpoints();

app.Run();
