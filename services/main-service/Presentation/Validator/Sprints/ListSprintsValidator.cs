using FluentValidation;
using TaskFlow.SprintService;

public class ListSprintsValidator : AbstractValidator<ListSprintsReq>
{
    public ListSprintsValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be greater than or equal to 1");

        RuleFor(x => x.Limit)
            .GreaterThanOrEqualTo(1).WithMessage("Limit must be greater than or equal to 1")
            .LessThanOrEqualTo(100).WithMessage("Limit cannot exceed 100 items per page");
    }
}