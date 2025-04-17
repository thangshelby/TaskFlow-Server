using FluentValidation;
using MainService.Domain.Enums;
using TaskFlow.IssueService;
using System;

public class UpdateIssueValidator : AbstractValidator<UpdateIssueReq>
{
    public UpdateIssueValidator()
    {
        // Validate URL path parameters
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Issue ID is required from URL path.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required from URL path.");

        // Optional fields validation
        RuleFor(x => x.Title)
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.Title));

        // Sprint and Assignee are optional but must be valid when provided
        When(x => !string.IsNullOrEmpty(x.SprintId), () =>
        {
            RuleFor(x => x.SprintId)
                .MaximumLength(50).WithMessage("Sprint ID must not exceed 50 characters.");
        });

        When(x => !string.IsNullOrEmpty(x.AssigneeId), () =>
        {
            RuleFor(x => x.AssigneeId)
                .MaximumLength(50).WithMessage("Assignee ID must not exceed 50 characters.");
        });

        RuleFor(x => x.Type)
            .Must(type => System.Enum.TryParse<IssueType>(type, true, out _))
            .WithMessage("Invalid issue type. Allowed values: Bug, Task, Story, Epic.")
            .When(x => !string.IsNullOrEmpty(x.Type));

        RuleFor(x => x.Status)
            .Must(status => System.Enum.TryParse<IssueStatus>(status, true, out _))
            .WithMessage("Invalid issue status. Allowed values: ToDo, InProgress, Done, Closed.")
            .When(x => !string.IsNullOrEmpty(x.Status));

        RuleFor(x => x.Priority)
            .Must(priority => System.Enum.TryParse<IssuePriority>(priority, true, out _))
            .WithMessage("Invalid priority. Allowed values: Low, Medium, High, Critical.")
            .When(x => !string.IsNullOrEmpty(x.Priority));

        RuleFor(x => x.Summary)
            .MaximumLength(500).WithMessage("Summary must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Summary));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.StoryPoint)
            .GreaterThanOrEqualTo(0).WithMessage("Story point must be a non-negative number.")
            .When(x => x.StoryPoint > 0);

        RuleForEach(x => x.Attachments)
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("Each attachment must be a valid URL.")
            .When(x => x.Attachments != null && x.Attachments.Count > 0);
    }
}
