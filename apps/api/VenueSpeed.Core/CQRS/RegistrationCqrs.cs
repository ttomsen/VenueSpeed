using MediatR;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Core.CQRS;

public record CreateRegistrationCommand(CreateRegistrationRequest Request, string AuthProviderId) : IRequest<Guid>;

public record CancelRegistrationCommand(Guid ExternalId, string AuthProviderId) : IRequest;

public record GetMyEventsQuery(string AuthProviderId) : IRequest<IReadOnlyList<MyEventDto>>;
