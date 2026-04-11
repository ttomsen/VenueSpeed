namespace VenueSpeed.Core.DTOs;

public record ParticipantDto(
    Guid ExternalId,
    string FirstName,
    DateOnly DateOfBirth,
    char Gender,
    string? City,
    string? StateCode,
    string? HeadlineText,
    string? ProfilePhotoUrl,
    string? Interests,
    bool IsActive,
    DateTime CreatedAtUtc
);

public record UpdateParticipantRequest(
    string FirstName,
    DateOnly DateOfBirth,
    char Gender,
    string? City,
    string? StateCode,
    string? HeadlineText,
    string? ProfilePhotoUrl,
    string? Interests
);
