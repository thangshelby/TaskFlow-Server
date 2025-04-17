namespace MainService.Domain.Enums;

public enum UserRole
{
    Admin,
    User,
}
public enum ProjectAccess
    {
        Public,
        Private,
        Restricted
    }
public enum ProjectType
    {
        Scrum,
        Kanban
    }
public enum IssueType
{
    Bug,
    Task,
    Story,
    Epic
}

public enum IssueStatus
{
    ToDo,
    InProgress,
    Done,
    Closed
}

public enum IssuePriority
{
    Low,
    Medium,
    High,
    Critical
}