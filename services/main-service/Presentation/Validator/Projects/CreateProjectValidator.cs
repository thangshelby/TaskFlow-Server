using FluentValidation;
using TaskFlow.ProjectService;

public class CreateProjectValidator : AbstractValidator<CreateProjectReq>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(100).WithMessage("Project name cannot exceed 100 characters");

        RuleFor(x => x.Key)
            .NotEmpty().WithMessage("Project key is required")
            .MaximumLength(50).WithMessage("Project key cannot exceed 50 characters");

        RuleFor(x => x.Access)
            .IsInEnum().WithMessage("Invalid access type");



        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required");
    }
}

public class UpdateProjectValidator : AbstractValidator<UpdateProjectReq>
{
    public UpdateProjectValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(100).WithMessage("Project name cannot exceed 100 characters");

        RuleFor(x => x.Access)
            .IsInEnum().WithMessage("Invalid access type");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid project type");

        RuleFor(x => x.OwnerId)
            .NotEmpty().WithMessage("Owner ID is required");
    }
}
