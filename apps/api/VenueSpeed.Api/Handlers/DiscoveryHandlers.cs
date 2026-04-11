using MediatR;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Api.Handlers;

public class GetNearbyEventsHandler : IRequestHandler<GetNearbyEventsQuery, IReadOnlyList<NearbyEventDto>>
{
    private readonly IDiscoveryRepository _repo;
    public GetNearbyEventsHandler(IDiscoveryRepository repo) => _repo = repo;

    public Task<IReadOnlyList<NearbyEventDto>> Handle(GetNearbyEventsQuery request, CancellationToken ct)
        => _repo.GetNearbyEventsAsync(request.Lat, request.Lng, request.RadiusMiles);
}

public class GetCityEventsHandler : IRequestHandler<GetCityEventsQuery, IReadOnlyList<NearbyEventDto>>
{
    private readonly IDiscoveryRepository _repo;
    public GetCityEventsHandler(IDiscoveryRepository repo) => _repo = repo;

    public Task<IReadOnlyList<NearbyEventDto>> Handle(GetCityEventsQuery request, CancellationToken ct)
        => _repo.GetEventsByCityAsync(request.City);
}

public class GetPublicVenueHandler : IRequestHandler<GetPublicVenueQuery, PublicVenueDto?>
{
    private readonly IDiscoveryRepository _repo;
    public GetPublicVenueHandler(IDiscoveryRepository repo) => _repo = repo;

    public Task<PublicVenueDto?> Handle(GetPublicVenueQuery request, CancellationToken ct)
        => _repo.GetPublicVenueBySlugAsync(request.Slug);
}
