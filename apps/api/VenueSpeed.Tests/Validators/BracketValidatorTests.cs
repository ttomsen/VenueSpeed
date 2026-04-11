using FluentValidation.TestHelper;
using VenueSpeed.Api.Validators;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Enums;
using Xunit;

namespace VenueSpeed.Tests.Validators;

public class CreateBracketRequestValidatorTests
{
    private readonly CreateBracketRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_PassesValidation()
    {
        var start = DateTime.UtcNow.AddHours(2);
        var request = new CreateBracketRequest(
            "Mixed 30s", 28, 39, GenderComposition.MF,
            start, start.AddHours(2), 180, 12, 25.00m);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void NegativeTicketPrice_FailsValidation()
    {
        var start = DateTime.UtcNow.AddHours(2);
        var request = new CreateBracketRequest(
            "Test", null, null, GenderComposition.ANY,
            start, start.AddHours(1), 180, 12, -5.00m);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TicketPriceUsd);
    }

    [Fact]
    public void EndBeforeStart_FailsValidation()
    {
        var start = DateTime.UtcNow.AddHours(2);
        var request = new CreateBracketRequest(
            "Test", null, null, GenderComposition.ANY,
            start, start.AddMinutes(-30), 180, 12, 20m);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.EndTimeUtc);
    }
}
