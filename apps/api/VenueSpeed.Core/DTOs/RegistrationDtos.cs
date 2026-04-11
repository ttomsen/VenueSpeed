using VenueSpeed.Core.Enums;

namespace VenueSpeed.Core.DTOs;

public record RegistrationDto(
    Guid ExternalId,
    Guid BracketExternalId,
    PaymentStatus PaymentStatus,
    CheckInStatus CheckInStatus,
    int? WaitlistPosition,
    DateTime CreatedAtUtc
);

public record CreateRegistrationRequest(
    Guid BracketExternalId,
    string StripePaymentIntentId
);

public record MyEventDto(
    Guid RegistrationExternalId,
    string EventName,
    string VenueName,
    string VenueCity,
    string BracketName,
    DateTime EventDateUtc,
    DateTime StartTimeUtc,
    PaymentStatus PaymentStatus,
    CheckInStatus CheckInStatus
);
