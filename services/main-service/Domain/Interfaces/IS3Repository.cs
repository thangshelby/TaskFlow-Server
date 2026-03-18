namespace MainService.Domain.Interfaces;

public interface IS3Repository
{
    Task<string> CreatePresignedURLImage(
        string projectId,
        string userId,
        string fileExtension,
        string contentType);
}

