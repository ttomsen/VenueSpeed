using VenueSpeed.Core.Enums;

namespace VenueSpeed.Core.DTOs;

public record BracketDto(
    Guid ExternalId,
    Guid EventExternalId,
    string BracketName,
    int? AgeRangeMin,
    int? AgeRangeMax,
    GenderComposition GenderComposition,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    int RoundDurationSeconds,
    int MaxParticipantsPerSide,
    decimal TicketPriceUsd,
    BracketStatus Status,
    DateTime? CheckInOpenUtc,
    DateTime? ActualStartUtc,
    DateTime? ActualEndUtc,
    int? CurrentRoundNumber
);

public record CreateBracketRequest(
    string BracketName,
    int? AgeRangeMin,
    int? AgeRangeMax,
    GenderComposition GenderComposition,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    int RoundDurationSeconds,
    int MaxParticipantsPerSide,
    decimal TicketPriceUsd
);

public record UpdateBracketRequest(
    string BracketName,
    int? AgeRangeMin,
    int? AgeRangeMax,
    GenderComposition GenderComposition,
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    int RoundDurationSeconds,
    int MaxParticipantsPerSide,
    decimal TicketPriceUsd,
    BracketStatus Status
);
