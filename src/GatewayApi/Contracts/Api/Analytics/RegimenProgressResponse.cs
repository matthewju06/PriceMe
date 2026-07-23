namespace PearlMetric.GatewayApi.Contracts.Api.Analytics;

public sealed record ColorTrendPointDto(
    Guid ScanRunId,
    DateTime CapturedAtUtc,
    int SequenceIndex,
    double L,
    double A,
    double B,
    double DeltaE,
    string? ShadeGuideValue,
    double ConfidenceScore);

public sealed record RegimenProgressResponse(
    Guid RegimenId,
    Guid? BaselineScanRunId,
    string? DeltaEFormula,
    string? AlgorithmVersion,
    IReadOnlyList<ColorTrendPointDto> Points);
