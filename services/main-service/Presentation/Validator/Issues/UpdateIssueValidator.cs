using FluentValidation;
using MainService.Domain.Entities;
using TaskFlow.IssueService;
using System;

public class UpdateIssueValidator : AbstractValidator<UpdateIssueReq>
{
    public UpdateIssueValidator()
    {
        RuleFor(x => x.IssueId)
            .NotEmpty().WithMessage("Issue ID is required.");

        RuleFor(x => x.Title)
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.")
            .When(x => x.Title != null);

        RuleFor(x => x.Type)
            .Must(type => Enum.TryParse<IssueType>(type, true, out _))
            .WithMessage("Invalid issue type. Allowed values: Bug, Task, Story, Epic.")
            .When(x => x.Type != null);

        RuleFor(x => x.Status)
            .Must(status => Enum.TryParse<IssueStatus>(status, true, out _))
            .WithMessage("Invalid issue status. Allowed values: Open, InProgress, Resolved, Closed.")
            .When(x => x.Status != null);

        RuleFor(x => x.Priority)
            .Must(priority => Enum.TryParse<IssuePriority>(priority, true, out _))
            .WithMessage("Invalid priority. Allowed values: Low, Medium, High, Critical.")
            .When(x => x.Priority != null);

        RuleFor(x => x.Summary)
            .MaximumLength(500).WithMessage("Summary must not exceed 500 characters.")
            .When(x => x.Summary != null);

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.")
            .When(x => x.Description != null);

        RuleFor(x => x.StoryPoint)
            .GreaterThanOrEqualTo(0).WithMessage("Story point must be a non-negative number.")
            .When(x => x.HasStoryPoint);

        RuleForEach(x => x.Attachments)
            .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
            .WithMessage("Each attachment must be a valid URL.")
            .When(x => x.Attachments != null);
    }
}
