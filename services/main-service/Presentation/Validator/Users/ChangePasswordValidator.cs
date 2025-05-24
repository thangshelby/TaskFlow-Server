using FluentValidation;
using TaskFlow.UserService;

namespace MainService.Presentation.Validator.Users
{
    public class ChangePasswordValidator : AbstractValidator<ChangePasswordReq>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage("Current password is required")
                .SetValidator(new PasswordValidator());

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .SetValidator(new PasswordValidator())
                .NotEqual(x => x.OldPassword).WithMessage("New password must be different from current password");
        }
    }
}