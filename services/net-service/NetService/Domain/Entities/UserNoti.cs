namespace NetService.Domain.Entities;

public class UserNoti
{
    public int Id { get; set; }
    public int UserId { get; set; } 
    public int NotificationId { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Notification Notification { get; set; } = null!;
}