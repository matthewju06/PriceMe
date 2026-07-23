using System.ComponentModel.DataAnnotations;
using PearlMetric.GatewayApi.Contracts.CvWorker;

namespace PearlMetric.GatewayApi.Contracts.Api.Frames;

public sealed class RegisterFrameMetadataRequest
{
    [Range(0, CvWorkerProtocol.MaxFramesPerRequest - 1)]
    public int SequenceIndex { get; init; }

    [Required]
    public DateTime CapturedAtUtc { get; init; }

    [MaxLength(260)]
    public string? OriginalFileName { get; init; }
}

public sealed class RegisterFramesRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(CvWorkerProtocol.MaxFramesPerRequest)]
    public IReadOnlyList<RegisterFrameMetadataRequest> Frames { get; init; } = [];
}
