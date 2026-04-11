using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Core.Interfaces;

public interface IEventRepository
{
    Task<IReadOnlyList<EventDto>> GetAllByVenueAsync(long venueId);
    Task<EventDto?> GetByExternalIdAsync(Guid externalId, long venueId);
    Task<Guid> CreateAsync(long venueId, CreateEventRequest request);
    Task UpdateAsync(Guid externalId, long venueId, UpdateEventRequest request);
    Task SoftDeleteAsync(Guid externalId, long venueId);
}
