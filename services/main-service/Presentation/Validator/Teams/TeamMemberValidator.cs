using FluentValidation;
using TaskFlow.TeamService;

namespace MainService.Presentation.Validator.Teams;

public class AddTeamMemberValidator : AbstractValidator<AddTeamMemberReq>
{
    public AddTeamMemberValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid team member role.");
    }
}

public class UpdateTeamMemberRoleValidator : AbstractValidator<UpdateTeamMemberRoleReq>
{
    public UpdateTeamMemberRoleValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid team member role.");
    }
}

public class RemoveTeamMemberValidator : AbstractValidator<RemoveTeamMemberReq>
{
    public RemoveTeamMemberValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}

public class ListTeamMembersValidator : AbstractValidator<ListTeamMembersReq>
{
    public ListTeamMembersValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Limit must be less than or equal to 100.");
    }
}