using Microsoft.AspNetCore.Mvc;
using PearlMetric.GatewayApi.Contracts.Api.Frames;
using PearlMetric.GatewayApi.Services;
using PearlMetric.GatewayApi.Storage;

namespace PearlMetric.GatewayApi.Endpoints;

public static class FrameEndpoints
{
    public static RouteGroupBuilder MapFrameEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/runs/{runId:guid}/frames").WithTags("Frames");

        group.MapPost("/", async (
                Guid runId,
                [FromForm] IFormFileCollection files,
                [FromForm] string? capturedAtUtc,
                FrameUploadService frames,
                CancellationToken cancellationToken) =>
            {
                DateTime? parsedCapturedAt = null;
                if (!string.IsNullOrWhiteSpace(capturedAtUtc)
                    && DateTime.TryParse(capturedAtUtc, out var parsed))
                {
                    parsedCapturedAt = parsed;
                }

                // Prefer the explicit "files" field; fall back to every uploaded part.
                IReadOnlyList<IFormFile> uploadFiles = files.GetFiles("files");
                if (uploadFiles.Count == 0)
                {
                    uploadFiles = files;
                }

                try
                {
                    var response = await frames.UploadAsync(
                        runId,
                        uploadFiles,
                        parsedCapturedAt,
                        cancellationToken);

                    return response is null
                        ? Results.NotFound(new { title = "Scan run not found.", runId })
                        : Results.Created($"/api/runs/{runId}/frames", response);
                }
                catch (ImageValidationException ex)
                {
                    return Results.ValidationProblem(new Dictionary<string, string[]>
                    {
                        ["files"] = [ex.Message]
                    });
                }
            })
            .DisableAntiforgery()
            .WithName("UploadFrames")
            .WithSummary("Upload scan frames")
            .WithDescription("Multipart upload of one or more JPEG/PNG frames for a Pending run. Form field name: files. Frames append with increasing sequence indexes. Call analyze after capture is finished.")
            .Accepts<IFormFileCollection>("multipart/form-data")
            .Produces<RegisterFramesResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem();

        return group;
    }
}
