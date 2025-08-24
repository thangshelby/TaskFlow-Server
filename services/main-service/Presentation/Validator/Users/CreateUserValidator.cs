using FluentValidation;
using MainService.Domain.Enums;
using TaskFlow.UserService;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
    }

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule)
    {
        return rule.NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}

public class CreateUserValidator : AbstractValidator<CreateUserReq>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.FirstName).ValidName();
        RuleFor(x => x.LastName).ValidName();
        RuleFor(x => x.Email).ValidEmail();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .SetValidator(new PasswordValidator());

        RuleFor(x => x.Role.ToString())
            .Must(role => Enum.IsDefined(typeof(UserRole), role))
            .WithMessage("Invalid role value");
    }
}

public class RegisterUserValidator : AbstractValidator<RegisterUserReq>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.FirstName).ValidName();
        RuleFor(x => x.LastName).ValidName();
        RuleFor(x => x.Email).ValidEmail();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .SetValidator(new PasswordValidator());

        RuleFor(x => x.PasswordConfirm)
            .NotEmpty().WithMessage("Password confirmation is required")
            .Equal(x => x.Password).WithMessage("Password and password confirmation do not match");
    }
}

public class UpdateUserValidator : AbstractValidator<UpdateUserReq>
{
    public UpdateUserValidator()
    {

        RuleFor(x => x.FirstName).ValidName().When(x => !string.IsNullOrEmpty(x.FirstName));
        RuleFor(x => x.LastName).ValidName().When(x => !string.IsNullOrEmpty(x.LastName));
        RuleFor(x => x.Email).ValidEmail().When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Password)
            .SetValidator(new PasswordValidator())
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.Role.ToString())
            .Must(role => Enum.IsDefined(typeof(UserRole), role))
            .WithMessage("Invalid role value")
            .When(x => !string.IsNullOrEmpty(x.Role.ToString()));
    }
}
