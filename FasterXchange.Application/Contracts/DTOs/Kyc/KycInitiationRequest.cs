namespace FasterXchange.Application.Contracts.DTOs.Kyc;

public class KycInitiationRequest
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty; // ISO country code
    public string? IdType { get; set; } // Passport, DriverLicense, NationalID
    public bool UseClientSdk { get; set; } = false; // If true, return SDK config instead of server-side flow
}

