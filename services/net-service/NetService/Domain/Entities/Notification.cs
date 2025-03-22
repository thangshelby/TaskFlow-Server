namespace NetService.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public required string NotiHeader { get; set; }
    public required string NotiContent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<UserNoti> Users { get; set; } = [];

}