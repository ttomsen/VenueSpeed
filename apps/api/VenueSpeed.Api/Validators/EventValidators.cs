using FluentValidation;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Api.Validators;

public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(x => x.EventName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EventDateUtc).GreaterThan(DateTime.UtcNow);
        RuleFor(x => x.DoorsOpenUtc).GreaterThan(DateTime.UtcNow)
            .LessThan(x => x.EventDateUtc).WithMessage("Doors must open before the event starts.");
    }
}

public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    public UpdateEventRequestValidator()
    {
        RuleFor(x => x.EventName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DoorsOpenUtc).LessThan(x => x.EventDateUtc)
            .WithMessage("Doors must open before the event starts.");
    }
}
