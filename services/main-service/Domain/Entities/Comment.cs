namespace MainService.Domain.Entities;

public class CommentDomain
{
    public string? Id { get; set; }
    public string? Content { get; set; }
    public required string IssueId { get; set; }
    public required string UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}