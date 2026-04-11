using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    /// <summary>Lists all venues (admin only).</summary>
    [HttpGet("venues")]
    public async Task<ActionResult<IReadOnlyList<AdminVenueDto>>> GetAllVenues()
        => Ok(await _mediator.Send(new GetAllVenuesQuery()));

    /// <summary>Returns detail for a venue by ExternalId (admin only).</summary>
    [HttpGet("venues/{externalId:guid}")]
    public async Task<ActionResult<VenueDto>> GetVenueDetail(Guid externalId)
    {
        var result = await _mediator.Send(new GetAdminVenueDetailQuery(externalId));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Suspends a venue — sets IsActive = false.</summary>
    [HttpPost("venues/{externalId:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid externalId)
    {
        await _mediator.Send(new SuspendVenueCommand(externalId));
        return NoContent();
    }

    /// <summary>Reinstates a suspended venue — sets IsActive = true.</summary>
    [HttpPost("venues/{externalId:guid}/reinstate")]
    public async Task<IActionResult> Reinstate(Guid externalId)
    {
        await _mediator.Send(new ReinstateVenueCommand(externalId));
        return NoContent();
    }
}
