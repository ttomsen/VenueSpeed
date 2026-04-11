using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Api.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenant;

    public EventController(IMediator mediator, ITenantContext tenant)
    {
        _mediator = mediator;
        _tenant = tenant;
    }

    /// <summary>Lists all events for this venue.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EventDto>>> GetAll()
        => Ok(await _mediator.Send(new GetEventsQuery(_tenant.VenueId)));

    /// <summary>Creates a new event.</summary>
    [HttpPost]
    public async Task<ActionResult<CreatedResponse>> Create([FromBody] CreateEventRequest request)
    {
        var externalId = await _mediator.Send(new CreateEventCommand(_tenant.VenueId, request));
        return CreatedAtAction(nameof(GetById), new { externalId }, new CreatedResponse(externalId));
    }

    /// <summary>Returns a single event by its ExternalId.</summary>
    [HttpGet("{externalId:guid}")]
    public async Task<ActionResult<EventDto>> GetById(Guid externalId)
    {
        var result = await _mediator.Send(new GetEventQuery(externalId, _tenant.VenueId));
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Updates an existing event.</summary>
    [HttpPut("{externalId:guid}")]
    public async Task<IActionResult> Update(Guid externalId, [FromBody] UpdateEventRequest request)
    {
        await _mediator.Send(new UpdateEventCommand(externalId, _tenant.VenueId, request));
        return NoContent();
    }

    /// <summary>Soft-deletes an event.</summary>
    [HttpDelete("{externalId:guid}")]
    public async Task<IActionResult> Delete(Guid externalId)
    {
        await _mediator.Send(new DeleteEventCommand(externalId, _tenant.VenueId));
        return NoContent();
    }
}
