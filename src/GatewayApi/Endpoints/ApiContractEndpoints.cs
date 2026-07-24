using PearlMetric.GatewayApi.Contracts.Api.Analytics;

namespace PearlMetric.GatewayApi.Endpoints;

/// <summary>
/// Public API route composition. Analytics remains a placeholder until PM-020.
/// </summary>
public static class ApiContractEndpoints
{
    public static IEndpointRouteBuilder MapApiContractEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api")
            .WithTags("PearlMetric API");

        api.MapPatientEndpoints();
        api.MapRegimenEndpoints();
        api.MapScanRunEndpoints();
        api.MapFrameEndpoints();
        MapAnalytics(api);

        app.MapInternalFrameEndpoints();

        return app;
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
            .WithSummary("Get regimen progress (placeholder)")
            .WithDescription("Will return whitening progress over time once analytics ships (PM-020). Currently returns 501.")
            .Produces<RegimenProgressResponse>(StatusCodes.Status501NotImplemented);
    }
}
