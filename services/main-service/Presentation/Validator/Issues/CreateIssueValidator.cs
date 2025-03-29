using FluentValidation;
using TaskFlow.IssueService;

public class CreateIssueValidator : AbstractValidator<CreateIssueReq>
{
    public CreateIssueValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.SprintId)
            .NotEmpty().WithMessage("Sprint ID is required.");

        RuleFor(x => x.AssigneeId)
            .NotEmpty().WithMessage("Assignee ID is required.");

        RuleFor(x => x.ParentId)
            .NotEmpty().WithMessage("Parent ID is required.");

        RuleFor(x => x.ReporterId)
            .NotEmpty().WithMessage("Reporter ID is required.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(type => new[] { "Bug", "Task", "Story", "Epic" }.Contains(type))
            .WithMessage("Invalid issue type. Allowed values: Bug, Task, Story, Epic.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => new[] { "Open", "InProgress", "Resolved", "Closed" }.Contains(status))
            .WithMessage("Invalid issue status. Allowed values: Open, InProgress, Resolved, Closed.");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .Must(priority => new[] { "Low", "Medium", "High", "Critical" }.Contains(priority))
            .WithMessage("Invalid priority. Allowed values: Low, Medium, High, Critical.");

        RuleFor(x => x.Summary)
            .NotEmpty().WithMessage("Summary is required.")
            .MaximumLength(500).WithMessage("Summary must not exceed 500 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.StoryPoint)
            .GreaterThanOrEqualTo(0).WithMessage("Story point must be a non-negative number.");

        RuleForEach(x => x.Attachments)
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("Each attachment must be a valid URL.");
    }
}
