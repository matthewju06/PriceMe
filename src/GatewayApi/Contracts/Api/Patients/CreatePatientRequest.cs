using System.ComponentModel.DataAnnotations;
using PearlMetric.GatewayApi.Models;

namespace PearlMetric.GatewayApi.Contracts.Api.Patients;

public sealed class CreatePatientRequest
{
    [Required]
    [MaxLength(Patient.DisplayNameMaxLength)]
    public string DisplayName { get; init; } = string.Empty;
}
