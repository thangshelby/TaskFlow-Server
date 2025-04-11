using FluentValidation;
using MainService.Domain.Entities;
using TaskFlow.IssueService;
using System;

public class CreateIssueValidator : AbstractValidator<CreateIssueReq>
{
    public CreateIssueValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        // Sprint and Assignee are optional
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

        // Reporter ID is now set by server, so we don't validate it here

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(type => System.Enum.TryParse<IssueType>(type, true, out _))
            .WithMessage("Invalid issue type. Allowed values: Bug, Task, Story, Epic.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => System.Enum.TryParse<IssueStatus>(status, true, out _))
            .WithMessage("Invalid issue status. Allowed values: Open, InProgress, Resolved, Closed.");

        RuleFor(x => x.Priority)
            .NotEmpty().WithMessage("Priority is required.")
            .Must(priority => System.Enum.TryParse<IssuePriority>(priority, true, out _))
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
            .WithMessage("Each attachment must be a valid URL.")
            .When(x => x.Attachments != null && x.Attachments.Count > 0);
    }
}
