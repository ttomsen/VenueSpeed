using VenueSpeed.Core.Enums;

namespace VenueSpeed.Core.DTOs;

public record EventDto(
    Guid ExternalId,
    string EventName,
    DateTime EventDateUtc,
    DateTime DoorsOpenUtc,
    EventStatus Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);

public record CreateEventRequest(
    string EventName,
    DateTime EventDateUtc,
    DateTime DoorsOpenUtc
);

public record UpdateEventRequest(
    string EventName,
    DateTime EventDateUtc,
    DateTime DoorsOpenUtc,
    EventStatus Status
);
