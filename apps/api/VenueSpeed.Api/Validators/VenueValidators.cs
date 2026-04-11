using FluentValidation;
using VenueSpeed.Core.DTOs;

namespace VenueSpeed.Api.Validators;

public class UpdateVenueRequestValidator : AbstractValidator<UpdateVenueRequest>
{
    public UpdateVenueRequestValidator()
    {
        RuleFor(x => x.VenueName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.AddressLine1).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.StateCode).NotEmpty().Length(2);
        RuleFor(x => x.PostalCode).NotEmpty().MaximumLength(20);
        RuleFor(x => x.VenueType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TokenPriceUsd).GreaterThan(0);
        RuleFor(x => x.Latitude).InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);
        RuleFor(x => x.Longitude).InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
    }
}
