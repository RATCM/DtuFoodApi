using System.Globalization;
using DtuFoodAPI.DTOs;
using FluentValidation;

namespace DtuFoodAPI.Validation;

public class ProductRegistryValidator : AbstractValidator<ProductRegistry>
{
    public ProductRegistryValidator()
    {

        RuleFor(x => x.Price)
            .Custom((x, y) =>
            {
                if (!decimal.TryParse(x, out var result))
                {
                    y.AddFailure("Price must be a valid number");
                    return;
                }

                if (result < 0)
                    y.AddFailure("Price must be greater or equal to 0");
            });

        RuleFor(x => x.Name)
            .MinimumLength(1)
            .MaximumLength(128);

        RuleFor(x => x.Description)
            .MaximumLength(1024);

        RuleFor(x => x.Category)
            .MaximumLength(128);
    }
}