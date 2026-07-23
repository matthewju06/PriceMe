using System.ComponentModel.DataAnnotations;
using PearlMetric.GatewayApi.Models;

namespace PearlMetric.GatewayApi.Contracts.Api.Regimens;

public sealed class UpdateRegimenRequest
{
    [Required]
    [MaxLength(WhiteningRegimen.ProductNameMaxLength)]
    public string ProductName { get; init; } = string.Empty;

    [Required]
    public DateTime StartedAtUtc { get; init; }

    [Range(1, 3650)]
    public int DurationDays { get; init; }

    [Range(1, 24 * 30)]
    public int ScheduledIntervalHours { get; init; }
}
