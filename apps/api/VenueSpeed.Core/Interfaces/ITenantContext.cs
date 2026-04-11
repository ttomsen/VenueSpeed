namespace VenueSpeed.Core.Interfaces;

public interface ITenantContext
{
    long VenueId { get; }
    Guid VenueExternalId { get; }
    string UserEmail { get; }
    bool IsAdmin { get; }
}
