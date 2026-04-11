using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Core.Interfaces;

public interface IEventBracketRepository
{
    Task<IReadOnlyList<BracketDto>> GetByEventAsync(Guid eventExternalId, long venueId);
    Task<BracketDto?> GetByExternalIdAsync(Guid bracketExternalId, long venueId);
    Task<Guid> CreateAsync(Guid eventExternalId, long venueId, CreateBracketRequest request);
    Task UpdateAsync(Guid bracketExternalId, long venueId, UpdateBracketRequest request);
    Task SoftDeleteAsync(Guid bracketExternalId, long venueId);
}
