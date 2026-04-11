using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Core.Interfaces;

public interface IVenueRepository
{
    Task<VenueDto?> GetByExternalIdAsync(Guid externalId, long venueId);
    Task<VenueDto?> GetBySlugAsync(string slug);
    Task UpdateAsync(long venueId, UpdateVenueRequest request);
    Task<IReadOnlyList<AdminVenueDto>> GetAllAsync();
    Task SuspendAsync(Guid externalId);
    Task ReinstateAsync(Guid externalId);
}
