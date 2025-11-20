using DtuFoodAPI.DTOs;
using FluentValidation;

namespace DtuFoodAPI.Validation;

public class AvailabilityRegistryValidator : AbstractValidator<AvailabilityRegistry>
{
    public AvailabilityRegistryValidator()
    {
        RuleFor(x => x.DayOfWeek)
            .IsEnumName(typeof(DayOfWeek));

        RuleFor(x => new { x.OpeningTime, x.ClosingTime })
            .Must(x => x.OpeningTime < x.ClosingTime)
            .WithMessage("Opening time must be earlier than the closing time");
    }
}