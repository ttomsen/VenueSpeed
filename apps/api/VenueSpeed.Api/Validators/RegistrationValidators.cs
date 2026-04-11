using FluentValidation;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Api.Validators;

public class CreateRegistrationRequestValidator : AbstractValidator<CreateRegistrationRequest>
{
    public CreateRegistrationRequestValidator()
    {
        RuleFor(x => x.BracketExternalId).NotEmpty();
        RuleFor(x => x.StripePaymentIntentId).NotEmpty().MaximumLength(100);
    }
}
