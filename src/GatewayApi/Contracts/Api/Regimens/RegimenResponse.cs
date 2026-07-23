namespace PearlMetric.GatewayApi.Contracts.Api.Regimens;

public sealed record RegimenResponse(
    Guid Id,
    Guid PatientId,
    string ProductName,
    DateTime StartedAtUtc,
    int DurationDays,
    int ScheduledIntervalHours);
