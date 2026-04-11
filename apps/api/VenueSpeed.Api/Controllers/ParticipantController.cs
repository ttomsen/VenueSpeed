using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Api.Controllers;

[ApiController]
[Route("api/participants")]
[Authorize]
public class ParticipantController : ControllerBase
{
    private readonly IMediator _mediator;

    public ParticipantController(IMediator mediator) => _mediator = mediator;

    private string AuthProviderId => User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("Could not identify user.");

    /// <summary>Returns the authenticated participant's profile.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<ParticipantDto>> GetMyProfile()
    {
        var result = await _mediator.Send(new GetParticipantProfileQuery(AuthProviderId));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Updates the authenticated participant's profile.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateParticipantRequest request)
    {
        await _mediator.Send(new UpdateParticipantCommand(AuthProviderId, request));
        return NoContent();
    }

    /// <summary>GDPR account deletion — anonymizes all personal data.</summary>
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteMyAccount()
    {
        await _mediator.Send(new DeleteParticipantCommand(AuthProviderId));
        return NoContent();
    }
}
