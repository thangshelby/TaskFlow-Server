using FluentValidation;
using TaskFlow.UserService;

public class LoginUserValidator : AbstractValidator<LoginUserReq>
{
    public LoginUserValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required");

        RuleFor(x => x.Password)
             .NotEmpty().WithMessage("Password is required")
             .SetValidator(new PasswordValidator());
    }
}