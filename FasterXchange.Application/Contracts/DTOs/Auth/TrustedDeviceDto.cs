using System;

namespace FasterXchange.Application.Contracts.DTOs.Auth;

public class TrustedDeviceDto
{
    public string DeviceId { get; set; } = string.Empty;
    public string? DeviceName { get; set; }
    public string? DeviceType { get; set; }
    public bool IsTrusted { get; set; }
    public DateTime? TrustedAt { get; set; }
    public DateTime FirstSeenAt { get; set; }
    public DateTime LastSeenAt { get; set; }
}
