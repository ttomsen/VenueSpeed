using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Core.Interfaces;

public interface IEventRegistrationRepository
{
    Task<Guid> CreateAsync(CreateRegistrationRequest request, string authProviderId);
    Task CancelAsync(Guid externalId, string authProviderId);
    Task<IReadOnlyList<MyEventDto>> GetUpcomingByParticipantAsync(string authProviderId);
}
