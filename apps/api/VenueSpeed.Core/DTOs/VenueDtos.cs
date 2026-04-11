namespace VenueSpeed.Core.DTOs;

public record VenueDto(
    Guid ExternalId,
    string VenueName,
    string Slug,
    string Email,
    string? PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string StateCode,
    string PostalCode,
    decimal? Latitude,
    decimal? Longitude,
    string VenueType,
    int? Capacity,
    bool StripeOnboardingComplete,
    decimal TokenPriceUsd,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);

public record UpdateVenueRequest(
    string VenueName,
    string? PhoneNumber,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string StateCode,
    string PostalCode,
    decimal? Latitude,
    decimal? Longitude,
    string VenueType,
    int? Capacity,
    decimal TokenPriceUsd
);

public record AdminVenueDto(
    Guid ExternalId,
    string VenueName,
    string Slug,
    string Email,
    string City,
    string StateCode,
    bool IsActive,
    bool StripeOnboardingComplete,
    DateTime CreatedAtUtc
);
