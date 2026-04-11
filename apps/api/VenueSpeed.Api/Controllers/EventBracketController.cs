using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Api.Controllers;

[ApiController]
[Route("api/events/{eventExternalId:guid}/brackets")]
[Authorize]
public class EventBracketController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenant;

    public EventBracketController(IMediator mediator, ITenantContext tenant)
    {
        _mediator = mediator;
        _tenant = tenant;
    }

    /// <summary>Lists brackets for an event.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BracketDto>>> GetAll(Guid eventExternalId)
        => Ok(await _mediator.Send(new GetBracketsQuery(eventExternalId, _tenant.VenueId)));

    /// <summary>Creates a new bracket under an event.</summary>
    [HttpPost]
    public async Task<ActionResult<CreatedResponse>> Create(Guid eventExternalId, [FromBody] CreateBracketRequest request)
    {
        var externalId = await _mediator.Send(new CreateBracketCommand(eventExternalId, _tenant.VenueId, request));
        return Created($"api/events/{eventExternalId}/brackets/{externalId}", new CreatedResponse(externalId));
    }

    /// <summary>Updates a bracket.</summary>
    [HttpPut("{bracketExternalId:guid}")]
    public async Task<IActionResult> Update(Guid bracketExternalId, [FromBody] UpdateBracketRequest request)
    {
        await _mediator.Send(new UpdateBracketCommand(bracketExternalId, _tenant.VenueId, request));
        return NoContent();
    }

    /// <summary>Soft-deletes a bracket.</summary>
    [HttpDelete("{bracketExternalId:guid}")]
    public async Task<IActionResult> Delete(Guid bracketExternalId)
    {
        await _mediator.Send(new DeleteBracketCommand(bracketExternalId, _tenant.VenueId));
        return NoContent();
    }
}
