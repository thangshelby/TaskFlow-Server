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
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .SetValidator(new PasswordValidator());

        RuleFor(x => x.Role.ToString())
            .Must(role => Enum.IsDefined(typeof(UserRole), role))
            .WithMessage("Invalid role value");
    }
}

public class UpdateUserValidator : AbstractValidator<UpdateUserReq>
{
    public UpdateUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.LastName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Invalid email format")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Password)
            .SetValidator(new PasswordValidator())
            .When(x => !string.IsNullOrEmpty(x.Password));

        RuleFor(x => x.Role.ToString())
            .Must(role => Enum.IsDefined(typeof(UserRole), role))
            .WithMessage("Invalid role value")
            .When(x => !string.IsNullOrEmpty(x.Role.ToString()));
    }
}
