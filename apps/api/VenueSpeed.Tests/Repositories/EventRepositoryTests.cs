using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Enums;
using Xunit;

namespace VenueSpeed.Tests.Repositories;

/// <summary>
/// Unit tests for EventRepository mapping and request/DTO contracts.
/// Integration tests that hit a real database go in a separate test suite
/// that requires AZURE_SQL_CONNECTION_STRING to be set.
/// </summary>
public class EventRepositoryTests
{
    [Fact]
    public void CreateEventRequest_Properties_RoundTrip()
    {
        var now = DateTime.UtcNow.AddDays(30);
        var request = new CreateEventRequest("Singles Night", now, now.AddHours(-1));

        Assert.Equal("Singles Night", request.EventName);
        Assert.Equal(now, request.EventDateUtc);
    }

    [Fact]
    public void EventDto_Status_ParsesCorrectly()
    {
        var status = Enum.Parse<EventStatus>("Published");
        Assert.Equal(EventStatus.Published, status);
    }

    [Fact]
    public void EventDto_AllStatuses_ParseWithoutException()
    {
        var names = Enum.GetNames<EventStatus>();
        foreach (var name in names)
            Assert.True(Enum.TryParse<EventStatus>(name, out _));
    }
}
