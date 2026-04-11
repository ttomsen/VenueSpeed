using FluentValidation.TestHelper;
using VenueSpeed.Api.Validators;
using VenueSpeed.Core.DTOs;
using Xunit;

namespace VenueSpeed.Tests.Validators;

public class CreateEventRequestValidatorTests
{
    private readonly CreateEventRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var future = DateTime.UtcNow.AddDays(30);
        var request = new CreateEventRequest("Speed Dating Night", future, future.AddHours(-2));
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_EventName_FailsValidation()
    {
        var future = DateTime.UtcNow.AddDays(30);
        var request = new CreateEventRequest("", future, future.AddHours(-1));
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.EventName);
    }

    [Fact]
    public void DoorsOpenAfterEvent_FailsValidation()
    {
        var future = DateTime.UtcNow.AddDays(30);
        // Doors open is AFTER event start — invalid
        var request = new CreateEventRequest("Test", future, future.AddHours(1));
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DoorsOpenUtc);
    }
}
