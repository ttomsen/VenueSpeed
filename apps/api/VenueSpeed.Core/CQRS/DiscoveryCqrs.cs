using MediatR;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Core.CQRS;

public record GetNearbyEventsQuery(double Lat, double Lng, double RadiusMiles) : IRequest<IReadOnlyList<NearbyEventDto>>;

public record GetCityEventsQuery(string City) : IRequest<IReadOnlyList<NearbyEventDto>>;

public record GetPublicVenueQuery(string Slug) : IRequest<PublicVenueDto?>;
