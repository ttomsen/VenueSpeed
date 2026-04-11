using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace VenueSpeed.Api.Middleware;

/// <summary>
/// Development-only middleware that injects a fake authenticated principal so
/// the API can be exercised locally without a real Azure Entra External ID token.
/// Registered in Program.cs only when ASPNETCORE_ENVIRONMENT=Development.
/// </summary>
public class DevAuthMiddleware(RequestDelegate next)
{
    private static readonly Guid DevVenueExternalId = new("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    public async Task InvokeAsync(HttpContext context)
    {
        var claims = new[]
        {
            new Claim("extension_VenueId",         "1"),
            new Claim("extension_VenueExternalId", DevVenueExternalId.ToString()),
            new Claim("emails",                    "dev@venuespeed.com"),
            new Claim(ClaimTypes.Role,             "Venue"),
        };

        var identity  = new ClaimsIdentity(claims, authenticationType: "DevAuth");
        var principal = new ClaimsPrincipal(identity);

        context.User = principal;

        await next(context);
    }
}
