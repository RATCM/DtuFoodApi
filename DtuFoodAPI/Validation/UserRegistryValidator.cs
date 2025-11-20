using DtuFoodAPI.DTOs;
using FluentValidation;

namespace DtuFoodAPI.Validation;

public class UserRegistryValidator : AbstractValidator<UserRegistry>
{
    public UserRegistryValidator()
    {
        RuleFor(x => x.Email)
            .MaximumLength(128)
            .EmailAddress();

        RuleFor(x => x.Password)
            .MinimumLength(8)
            .MaximumLength(128)
            .Custom((x, y) =>
            {
                if(!x.Any(char.IsDigit))
                    y.AddFailure("Password must contain at least 1 number");
                
                if(!x.Any(char.IsLetter))
                    y.AddFailure("Password must contain at least 1 letter");
                else if(!(x.Any(char.IsLower) && x.Any(char.IsUpper)))
                    y.AddFailure("Password must contain both lowercase and uppercase letters");
            });
    }
}