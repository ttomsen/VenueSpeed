using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Api.Controllers;

[ApiController]
[Route("api/venues")]
[Authorize]
public class VenueController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenant;

    public VenueController(IMediator mediator, ITenantContext tenant)
    {
        _mediator = mediator;
        _tenant = tenant;
    }

    /// <summary>Returns the authenticated venue's own profile.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<VenueDto>> GetMyProfile()
    {
        var result = await _mediator.Send(new GetVenueProfileQuery(_tenant.VenueExternalId, _tenant.VenueId));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Updates the authenticated venue's profile.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateVenueRequest request)
    {
        await _mediator.Send(new UpdateVenueCommand(_tenant.VenueId, request));
        return NoContent();
    }
}
