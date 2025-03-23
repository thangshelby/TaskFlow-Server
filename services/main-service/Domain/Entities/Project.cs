using MainService.Domain.Enums;
namespace MainService.Domain.Entities;

public class ProjectDomain
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public ProjectAccess Access { get; set; }
    public ProjectType Type { get; set; } 
    public string OwnerId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
