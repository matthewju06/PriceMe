using PearlMetric.GatewayApi.Contracts.Api.Analytics;
using PearlMetric.GatewayApi.Contracts.Api.Frames;
using PearlMetric.GatewayApi.Contracts.Api.Patients;
using PearlMetric.GatewayApi.Contracts.Api.Regimens;
using PearlMetric.GatewayApi.Contracts.Api.Runs;
using PearlMetric.GatewayApi.Validation;

namespace PearlMetric.GatewayApi.Endpoints;

/// <summary>
/// Contract and validation surface for the public API.
/// Persistence is implemented in later stories (PM-008+); valid requests currently
/// return placeholder responses so OpenAPI documents the shapes and invalid bodies
/// return RFC 7807 validation problems.
/// </summary>
public static class ApiContractEndpoints
{
    public static IEndpointRouteBuilder MapApiContractEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api")
            .WithTags("PearlMetric API");

        MapPatients(api);
        MapRegimens(api);
        MapRuns(api);
        MapFrames(api);
        MapAnalytics(api);

        return app;
    }

    private static void MapPatients(RouteGroupBuilder api)
    {
        var group = api.MapGroup("/patients").WithTags("Patients");

        group.MapPost("/", (CreatePatientRequest request) =>
            {
                if (RequestValidator.Validate(request) is { } problem)
                {
                    return problem;
                }

                var response = new PatientResponse(
                    Guid.NewGuid(),
                    request.DisplayName.Trim(),
                    DateTime.UtcNow);

                return Results.Created($"/api/patients/{response.Id}", response);
            })
            .WithName("CreatePatient")
            .Produces<PatientResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{patientId:guid}", (Guid patientId) =>
                Results.Json(
                    new PatientResponse(patientId, "placeholder", DateTime.UtcNow),
                    statusCode: StatusCodes.Status501NotImplemented))
            .WithName("GetPatient")
            .Produces<PatientResponse>(StatusCodes.Status501NotImplemented);
    }

    private static void MapRegimens(RouteGroupBuilder api)
    {
        var group = api.MapGroup("/regimens").WithTags("Regimens");

        group.MapPost("/", (CreateRegimenRequest request) =>
            {
                if (RequestValidator.Validate(request) is { } problem)
                {
                    return problem;
                }

                var response = new RegimenResponse(
                    Guid.NewGuid(),
                    request.PatientId,
                    request.ProductName.Trim(),
                    DateTime.SpecifyKind(request.StartedAtUtc, DateTimeKind.Utc),
                    request.DurationDays,
                    request.ScheduledIntervalHours);

                return Results.Created($"/api/regimens/{response.Id}", response);
            })
            .WithName("CreateRegimen")
            .Produces<RegimenResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{regimenId:guid}", (Guid regimenId) =>
                Results.Json(
                    new RegimenResponse(regimenId, Guid.Empty, "placeholder", DateTime.UtcNow, 1, 24),
                    statusCode: StatusCodes.Status501NotImplemented))
            .WithName("GetRegimen")
            .Produces<RegimenResponse>(StatusCodes.Status501NotImplemented);

        group.MapPut("/{regimenId:guid}", (Guid regimenId, UpdateRegimenRequest request) =>
            {
                if (RequestValidator.Validate(request) is { } problem)
                {
                    return problem;
                }

                var response = new RegimenResponse(
                    regimenId,
                    Guid.Empty,
                    request.ProductName.Trim(),
                    DateTime.SpecifyKind(request.StartedAtUtc, DateTimeKind.Utc),
                    request.DurationDays,
                    request.ScheduledIntervalHours);

                return Results.Json(response, statusCode: StatusCodes.Status501NotImplemented);
            })
            .WithName("UpdateRegimen")
            .Produces<RegimenResponse>(StatusCodes.Status501NotImplemented)
            .ProducesValidationProblem();
    }

    private static void MapRuns(RouteGroupBuilder api)
    {
        var group = api.MapGroup("/runs").WithTags("Runs");

        group.MapPost("/", (CreateScanRunRequest request) =>
            {
                if (RequestValidator.Validate(request) is { } problem)
                {
                    return problem;
                }

                var response = new ScanRunResponse(
                    Guid.NewGuid(),
                    request.RegimenId,
                    DateTime.SpecifyKind(request.CapturedAtUtc, DateTimeKind.Utc),
                    request.DeviceId.Trim(),
                    Models.ScanRunStatus.Pending,
                    BaselineScanRunId: null,
                    DeltaEFormula: null,
                    AlgorithmVersion: null,
                    FailureReason: null,
                    FrameCount: 0,
                    SampleCount: 0);

                return Results.Created($"/api/runs/{response.Id}", response);
            })
            .WithName("CreateScanRun")
            .Produces<ScanRunResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("/{runId:guid}", (Guid runId) =>
                Results.Json(
                    new ScanRunResponse(
                        runId,
                        Guid.Empty,
                        DateTime.UtcNow,
                        "placeholder",
                        Models.ScanRunStatus.Pending,
                        null,
                        null,
                        null,
                        null,
                        0,
                        0),
                    statusCode: StatusCodes.Status501NotImplemented))
            .WithName("GetScanRun")
            .Produces<ScanRunResponse>(StatusCodes.Status501NotImplemented);
    }

    private static void MapFrames(RouteGroupBuilder api)
    {
        var group = api.MapGroup("/runs/{runId:guid}/frames").WithTags("Frames");

        group.MapPost("/", (Guid runId, RegisterFramesRequest request) =>
            {
                if (RequestValidator.Validate(request) is { } problem)
                {
                    return problem;
                }

                foreach (var frame in request.Frames)
                {
                    if (RequestValidator.Validate(frame) is { } frameProblem)
                    {
                        return frameProblem;
                    }
                }

                var frames = request.Frames
                    .Select(frame => new FrameResponse(
                        Guid.NewGuid(),
                        runId,
                        frame.SequenceIndex,
                        StorageKey: $"runs/{runId}/{frame.SequenceIndex:D4}.jpg",
                        ContentType: "image/jpeg",
                        ByteSize: 1,
                        DateTime.SpecifyKind(frame.CapturedAtUtc, DateTimeKind.Utc),
                        frame.OriginalFileName))
                    .ToArray();

                return Results.Json(
                    new RegisterFramesResponse(runId, frames),
                    statusCode: StatusCodes.Status501NotImplemented);
            })
            .WithName("RegisterFrames")
            .Produces<RegisterFramesResponse>(StatusCodes.Status501NotImplemented)
            .ProducesValidationProblem();
    }

    private static void MapAnalytics(RouteGroupBuilder api)
    {
        var group = api.MapGroup("/analytics").WithTags("Analytics");

        group.MapGet("/regimens/{regimenId:guid}/progress", (Guid regimenId) =>
                Results.Json(
                    new RegimenProgressResponse(
                        regimenId,
                        BaselineScanRunId: null,
                        DeltaEFormula: null,
                        AlgorithmVersion: null,
                        Points: []),
                    statusCode: StatusCodes.Status501NotImplemented))
            .WithName("GetRegimenProgress")
            .Produces<RegimenProgressResponse>(StatusCodes.Status501NotImplemented);
    }
}
