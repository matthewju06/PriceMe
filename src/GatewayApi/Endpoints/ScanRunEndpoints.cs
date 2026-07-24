using PearlMetric.GatewayApi.Contracts.Api.Runs;
using PearlMetric.GatewayApi.Services;
using PearlMetric.GatewayApi.Validation;

namespace PearlMetric.GatewayApi.Endpoints;

public static class ScanRunEndpoints
{
    public static RouteGroupBuilder MapScanRunEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/runs").WithTags("Runs");

        group.MapPost("/", async (
                CreateScanRunRequest request,
                ScanRunService runs,
                CancellationToken cancellationToken) =>
            {
                if (RequestValidator.Validate(request) is { } problem)
                {
                    return problem;
                }

                if (request.RegimenId == Guid.Empty)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["RegimenId"] = ["RegimenId is required."]
                    });
                }

                var response = await runs.CreateAsync(request, cancellationToken);
                return response is null
                    ? Results.NotFound(new { title = "Regimen not found.", regimenId = request.RegimenId })
                    : Results.Created($"/api/runs/{response.Id}", response);
            })
            .WithName("CreateScanRun")
            .WithSummary("Create scan run")
            .WithDescription("Opens a Pending capture session for one visit/device. Upload frames next, then call analyze.")
            .Produces<ScanRunResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapGet("/{runId:guid}", async (
                Guid runId,
                ScanRunService runs,
                CancellationToken cancellationToken) =>
            {
                var response = await runs.GetByIdAsync(runId, cancellationToken);
                return response is null ? Results.NotFound() : Results.Ok(response);
            })
            .WithName("GetScanRun")
            .WithSummary("Get scan run")
            .WithDescription("Returns run status and frame/sample counts. Use to poll after upload or analysis.")
            .Produces<ScanRunResponse>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{runId:guid}/analyze", async (
                Guid runId,
                ScanAnalysisService analysis,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var response = await analysis.AnalyzeAsync(runId, cancellationToken);
                    return response is null ? Results.NotFound() : Results.Ok(response);
                }
                catch (InvalidOperationException ex)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["run"] = [ex.Message]
                    });
                }
            })
            .WithName("AnalyzeScanRun")
            .WithSummary("Analyze scan run")
            .WithDescription("Runs shade analysis on uploaded frames (fake CV in local MVP). Transitions Pending→Processing→Completed/Failed, persists calibration and Lab/DeltaE samples, and links DeltaE to the regimen's first completed scan when present. Idempotent once Completed.")
            .Produces<ScanRunResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        group.MapGet("/{runId:guid}/analysis", async (
                Guid runId,
                ScanRunService runs,
                CancellationToken cancellationToken) =>
            {
                var detail = await runs.GetAnalysisDetailAsync(runId, cancellationToken);
                return detail is null ? Results.NotFound() : Results.Ok(detail);
            })
            .WithName("GetScanRunAnalysis")
            .WithSummary("Get analysis detail")
            .WithDescription("Returns calibration profile and color metric samples for charts, shade comparison, and clinician review after a successful analyze.")
            .Produces<ScanRunAnalysisDetailResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
