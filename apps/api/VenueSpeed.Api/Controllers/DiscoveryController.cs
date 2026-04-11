using MediatR;
using Microsoft.AspNetCore.Mvc;
using VenueSpeed.Core.CQRS;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Api.Controllers;

[ApiController]
[Route("api/discovery")]
public class DiscoveryController : ControllerBase
{
    private readonly IMediator _mediator;

    public DiscoveryController(IMediator mediator) => _mediator = mediator;

    /// <summary>Returns upcoming events within the given radius of a coordinate.</summary>
    [HttpGet("events")]
    public async Task<ActionResult<IReadOnlyList<NearbyEventDto>>> GetNearbyEvents(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radiusMiles = 25)
    {
        if (lat is < -90 or > 90 || lng is < -180 or > 180)
            return BadRequest(new ErrorResponse("Invalid coordinates.", "INVALID_COORDINATES", Guid.NewGuid()));

        return Ok(await _mediator.Send(new GetNearbyEventsQuery(lat, lng, radiusMiles)));
    }

    /// <summary>Returns upcoming events in a city.</summary>
    [HttpGet("cities/{city}/events")]
    public async Task<ActionResult<IReadOnlyList<NearbyEventDto>>> GetCityEvents(string city)
        => Ok(await _mediator.Send(new GetCityEventsQuery(city)));

    /// <summary>Returns the public venue page for a given slug.</summary>
    [HttpGet("venues/{slug}")]
    public async Task<ActionResult<PublicVenueDto>> GetPublicVenue(string slug)
    {
        var result = await _mediator.Send(new GetPublicVenueQuery(slug));
        return result is null ? NotFound() : Ok(result);
    }
}
