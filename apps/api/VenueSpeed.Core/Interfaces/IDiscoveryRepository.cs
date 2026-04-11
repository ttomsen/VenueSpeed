using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Core.Interfaces;

public interface IDiscoveryRepository
{
    Task<IReadOnlyList<NearbyEventDto>> GetNearbyEventsAsync(double lat, double lng, double radiusMiles);
    Task<IReadOnlyList<NearbyEventDto>> GetEventsByCityAsync(string city);
    Task<PublicVenueDto?> GetPublicVenueBySlugAsync(string slug);
}
