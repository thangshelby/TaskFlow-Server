namespace MainService.Domain.Enums;

public enum NotificationType
{
    IssueAssigned = 0,
    IssueUpdated = 1,
    CommentMention = 2,
    SprintStarting = 3,
    ProjectInvitation = 4,
    TeamMemberAdded = 5
}