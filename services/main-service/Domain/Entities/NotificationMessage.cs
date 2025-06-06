using MainService.Domain.Enums;

namespace MainService.Domain.Entities;

public class NotificationMessageDomain
{
    public string? Id { get; set; }
    public required string UserId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required NotificationType Type { get; set; }
    public required string ReferenceId { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}