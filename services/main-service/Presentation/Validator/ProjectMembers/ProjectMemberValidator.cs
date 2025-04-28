using FluentValidation;
using TaskFlow.ProjectMemberService;

namespace MainService.Presentation.Validator.ProjectMembers;

public class AddProjectMemberValidator : AbstractValidator<AddProjectMemberReq>
{
    public AddProjectMemberValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid project member role.");
    }
}

public class UpdateProjectMemberRoleValidator : AbstractValidator<UpdateProjectMemberRoleReq>
{
    public UpdateProjectMemberRoleValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid project member role.");
    }
}

public class RemoveProjectMemberValidator : AbstractValidator<RemoveProjectMemberReq>
{
    public RemoveProjectMemberValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}

public class ListProjectMembersValidator : AbstractValidator<ListProjectMembersReq>
{
    public ListProjectMembersValidator()
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

public class UserMembershipsValidator : AbstractValidator<UserMembershipsReq>
{
    public UserMembershipsValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page must be greater than 0.");

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Limit must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Limit must be less than or equal to 100.");
    }
}

public class ApproveMemberValidator : AbstractValidator<ApproveMemberReq>
{
    public ApproveMemberValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}

public class RejectMemberValidator : AbstractValidator<RejectMemberReq>
{
    public RejectMemberValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}