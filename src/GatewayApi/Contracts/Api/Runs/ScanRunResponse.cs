using PearlMetric.GatewayApi.Models;

namespace PearlMetric.GatewayApi.Contracts.Api.Runs;

public sealed record ScanRunResponse(
    Guid Id,
    Guid RegimenId,
    DateTime CapturedAtUtc,
    string DeviceId,
    ScanRunStatus Status,
    Guid? BaselineScanRunId,
    string? DeltaEFormula,
    string? AlgorithmVersion,
    string? FailureReason,
    int FrameCount,
    int SampleCount);
