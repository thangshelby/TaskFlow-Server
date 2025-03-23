namespace NetService.Application.DTOs.Notifications;

public class NotificationDto
{
    public int Id { get; set; }
    public string NotiHeader { get; set; } = string.Empty;
    public string NotiContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
