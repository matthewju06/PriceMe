namespace PearlMetric.GatewayApi.Contracts.Api.Patients;

public sealed record PatientResponse(
    Guid Id,
    string DisplayName,
    DateTime CreatedAtUtc);
