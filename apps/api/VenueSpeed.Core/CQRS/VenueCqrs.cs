using MediatR;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Core.CQRS;

public record GetVenueProfileQuery(Guid ExternalId, long VenueId) : IRequest<VenueDto?>;

public record UpdateVenueCommand(long VenueId, UpdateVenueRequest Request) : IRequest;

public record GetAllVenuesQuery : IRequest<IReadOnlyList<AdminVenueDto>>;

public record GetAdminVenueDetailQuery(Guid ExternalId) : IRequest<VenueDto?>;

public record SuspendVenueCommand(Guid ExternalId) : IRequest;

public record ReinstateVenueCommand(Guid ExternalId) : IRequest;
