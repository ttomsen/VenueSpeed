using MediatR;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Core.CQRS;

public record GetEventsQuery(long VenueId) : IRequest<IReadOnlyList<EventDto>>;

public record GetEventQuery(Guid ExternalId, long VenueId) : IRequest<EventDto?>;

public record CreateEventCommand(long VenueId, CreateEventRequest Request) : IRequest<Guid>;

public record UpdateEventCommand(Guid ExternalId, long VenueId, UpdateEventRequest Request) : IRequest;

public record DeleteEventCommand(Guid ExternalId, long VenueId) : IRequest;
