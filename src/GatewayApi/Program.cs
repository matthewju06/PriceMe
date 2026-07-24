using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using PearlMetric.GatewayApi.Configuration;
using PearlMetric.GatewayApi.Data;
using PearlMetric.GatewayApi.Endpoints;
using PearlMetric.GatewayApi.Services;
using PearlMetric.GatewayApi.Services.Cv;
using PearlMetric.GatewayApi.Storage;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("PearlMetric")
    ?? throw new InvalidOperationException(
        "Connection string 'PearlMetric' is required. Configure it with user secrets or the ConnectionStrings__PearlMetric environment variable.");

builder.Services.AddDbContext<PearlMetricDb>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<PatientService>();
builder.Services.AddScoped<RegimenService>();
builder.Services.AddScoped<ScanRunService>();
builder.Services.AddScoped<FrameUploadService>();
builder.Services.AddScoped<ScanAnalysisService>();
builder.Services.AddSingleton<IImageStore, LocalFileImageStore>();
builder.Services.AddSingleton<ICvAnalysisClient, FakeCvAnalysisClient>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

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
    .Validate(
        options => options.MaxFrameBytes > 0,
        "ImageStorage:MaxFrameBytes must be greater than zero.")
    .Validate(
        options => options.MaxFramesPerRun is > 0 and <= 32,
        "ImageStorage:MaxFramesPerRun must be between 1 and 32.")
    .ValidateOnStart();

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, _, _) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "PearlMetric Gateway API",
            Version = "v1",
            Description =
                "Local MVP API for patient regimens, scan capture, frame upload, and shade analysis. " +
                "Typical clinic flow: create patient → start regimen → create scan run → upload frames → analyze → read analysis."
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("PearlMetric API");
    });
}

app.UseHttpsRedirection();

app.MapGet("/health", () => Results.Ok(new
{
    Status = "OK",
    Timestamp = DateTime.UtcNow
}))
.WithName("Health")
.WithSummary("Health check")
.WithDescription("Liveness probe for local runs and orchestration. Not patient-facing.");

app.MapApiContractEndpoints();

app.Run();
