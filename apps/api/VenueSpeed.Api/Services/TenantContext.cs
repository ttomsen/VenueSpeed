using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Api.Services;

public class TenantContext : ITenantContext
{
    public long VenueId { get; set; }
    public Guid VenueExternalId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
}
