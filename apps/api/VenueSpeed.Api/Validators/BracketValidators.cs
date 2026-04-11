using FluentValidation;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Api.Validators;

public class CreateBracketRequestValidator : AbstractValidator<CreateBracketRequest>
{
    public CreateBracketRequestValidator()
    {
        RuleFor(x => x.BracketName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoundDurationSeconds).InclusiveBetween(60, 900);
        RuleFor(x => x.MaxParticipantsPerSide).InclusiveBetween(1, 50);
        RuleFor(x => x.TicketPriceUsd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.EndTimeUtc).GreaterThan(x => x.StartTimeUtc)
            .WithMessage("End time must be after start time.");
        RuleFor(x => x.AgeRangeMax).GreaterThan(x => x.AgeRangeMin)
            .When(x => x.AgeRangeMin.HasValue && x.AgeRangeMax.HasValue)
            .WithMessage("AgeRangeMax must be greater than AgeRangeMin.");
    }
}
