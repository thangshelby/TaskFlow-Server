using MainService.Domain.Entities;
public class IActivitiesMessage
{
    public IssueDomain? OldIssue { get; set; }
    public required IssueDomain NewIssue { get; set; }
    public required string EventType { get; set; }
};

public static class ActivitiesMessageAction
{
    public const string ISSUE_CHANGED = "ISSUE_CHANGED_ACTION";
    public const string ISSUE_CREATED = "ISSUE_CREATED_ACTION";
}