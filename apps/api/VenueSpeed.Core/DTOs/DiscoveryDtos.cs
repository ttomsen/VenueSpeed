using VenueSpeed.Core.Enums;

namespace VenueSpeed.Core.DTOs;

public record NearbyEventDto(
    Guid EventExternalId,
    string EventName,
    DateTime EventDateUtc,
    string VenueName,
    string VenueSlug,
    string City,
    string StateCode,
    decimal? Latitude,
    decimal? Longitude,
    double? DistanceMiles,
    IReadOnlyList<BracketSummaryDto> Brackets
);

public record BracketSummaryDto(
    Guid ExternalId,
    string BracketName,
    GenderComposition GenderComposition,
    DateTime StartTimeUtc,
    decimal TicketPriceUsd,
    BracketStatus Status
);

public record PublicVenueDto(
    Guid ExternalId,
    string VenueName,
    string Slug,
    string City,
    string StateCode,
    string? Description,
    string? LogoUrl,
    string? CoverPhotoUrl,
    string? GoogleMapsUrl,
    decimal TokenPriceUsd,
    IReadOnlyList<NearbyEventDto> UpcomingEvents
);
