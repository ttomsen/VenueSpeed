namespace VenueSpeed.Core.DTOs;

public record ErrorResponse(string Error, string Code, Guid TraceId);

public record CreatedResponse(Guid ExternalId);
