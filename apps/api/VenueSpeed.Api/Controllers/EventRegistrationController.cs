using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Api.Controllers;

[ApiController]
[Route("api/registrations")]
[Authorize]
public class EventRegistrationController : ControllerBase
{
    private readonly IMediator _mediator;

    public EventRegistrationController(IMediator mediator) => _mediator = mediator;

    private string AuthProviderId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("Could not identify user.");

    /// <summary>Registers a participant for a bracket.</summary>
    [HttpPost]
    public async Task<ActionResult<CreatedResponse>> Register([FromBody] CreateRegistrationRequest request)
    {
        var externalId = await _mediator.Send(new CreateRegistrationCommand(request, AuthProviderId));
        return Created($"api/registrations/{externalId}", new CreatedResponse(externalId));
    }

    /// <summary>Cancels a registration.</summary>
    [HttpDelete("{externalId:guid}")]
    public async Task<IActionResult> Cancel(Guid externalId)
    {
        await _mediator.Send(new CancelRegistrationCommand(externalId, AuthProviderId));
        return NoContent();
    }

    /// <summary>Returns upcoming events for the authenticated participant.</summary>
    [HttpGet("my-events")]
    public async Task<ActionResult<IReadOnlyList<MyEventDto>>> GetMyEvents()
        => Ok(await _mediator.Send(new GetMyEventsQuery(AuthProviderId)));
}
