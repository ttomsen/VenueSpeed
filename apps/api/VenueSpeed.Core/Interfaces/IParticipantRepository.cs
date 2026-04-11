using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Core.Interfaces;

public interface IParticipantRepository
{
    Task<ParticipantDto?> GetByAuthProviderIdAsync(string authProviderId);
    Task UpdateAsync(string authProviderId, UpdateParticipantRequest request);
    Task GdprDeleteAsync(string authProviderId);
}
