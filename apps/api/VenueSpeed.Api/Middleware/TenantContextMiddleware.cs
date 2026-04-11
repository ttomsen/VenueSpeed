using System.Security.Claims;
using VenueSpeed.Api.Services;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Api.Middleware;

/// <summary>
/// Reads VenueId, VenueExternalId, email, and role from the validated JWT
/// and populates ITenantContext for downstream handlers and repositories.
/// </summary>
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated == true && tenantContext is TenantContext tc)
        {
            var venueIdClaim = context.User.FindFirstValue("extension_VenueId");
            var venueExternalIdClaim = context.User.FindFirstValue("extension_VenueExternalId");
            var emailClaim = context.User.FindFirstValue("emails")
                ?? context.User.FindFirstValue(ClaimTypes.Email)
                ?? string.Empty;

            if (long.TryParse(venueIdClaim, out var venueId))
                tc.VenueId = venueId;

            if (Guid.TryParse(venueExternalIdClaim, out var venueExternalId))
                tc.VenueExternalId = venueExternalId;

            tc.UserEmail = emailClaim;
            tc.IsAdmin = context.User.IsInRole("Admin") ||
                         context.User.HasClaim("extension_IsAdmin", "true");
        }

        await _next(context);
    }
}
