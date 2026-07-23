namespace PearlMetric.GatewayApi.Contracts.Api.Frames;

public sealed record FrameResponse(
    Guid Id,
    Guid ScanRunId,
    int SequenceIndex,
    string StorageKey,
    string ContentType,
    long ByteSize,
    DateTime CapturedAtUtc,
    string? OriginalFileName);

public sealed record RegisterFramesResponse(
    Guid ScanRunId,
    IReadOnlyList<FrameResponse> Frames);
