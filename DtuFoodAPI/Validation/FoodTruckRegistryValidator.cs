using DtuFoodAPI.DTOs;
using FluentValidation;

namespace DtuFoodAPI.Validation;

public class FoodTruckRegistryValidator : AbstractValidator<FoodTruckRegistry>
{
    public FoodTruckRegistryValidator()
    {
        RuleFor(x => x.Name)
            .MinimumLength(1)
            .MaximumLength(128);

        RuleFor(x => x.GpsLatitude)
            .GreaterThan(-180)
            .LessThan(180);
        
        RuleFor(x => x.GpsLongitude)
            .GreaterThan(-180)
            .LessThan(180);
    }
}