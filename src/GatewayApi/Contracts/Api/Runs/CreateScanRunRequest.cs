using System.ComponentModel.DataAnnotations;
using PearlMetric.GatewayApi.Models;

namespace PearlMetric.GatewayApi.Contracts.Api.Runs;

public sealed class CreateScanRunRequest
{
    [Required]
    public Guid RegimenId { get; init; }

    [Required]
    public DateTime CapturedAtUtc { get; init; }

    [Required]
    [MaxLength(ScanRun.DeviceIdMaxLength)]
    public string DeviceId { get; init; } = string.Empty;
}
