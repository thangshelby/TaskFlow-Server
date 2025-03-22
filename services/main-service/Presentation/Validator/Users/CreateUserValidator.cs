using FluentValidation;
using MainService.Domain.Enums;
using TaskFlow.UserService;

public class CreateUserValidator : AbstractValidator<CreateUserReq>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .SetValidator(new PasswordValidator());

        RuleFor(x => x.Role.ToString())
            .Must(role => Enum.IsDefined(typeof(UserRole), role))
            .WithMessage("Invalid role value");
    }
}
