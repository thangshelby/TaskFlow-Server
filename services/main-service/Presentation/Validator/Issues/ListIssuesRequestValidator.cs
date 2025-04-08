using FluentValidation;
using TaskFlow.IssueService;

namespace MainService.Presentation.Validator;

public class ListIssuesRequestValidator : AbstractValidator<ListIssuesReq>
{
    public ListIssuesRequestValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required");

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.Limit)
            .GreaterThan(0).WithMessage("Page size must be greater than 0");
    }
}