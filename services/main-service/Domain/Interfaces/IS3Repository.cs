namespace MainService.Domain.Interfaces;

public interface IS3Repository
{
    /// <summary>
    /// Tạo presigned PUT URL để upload file lên S3.
    /// Trả về (presignedUrl, fileUrl):
    ///   - presignedUrl: URL có chữ ký dùng để PUT file (ngắn hạn)
    ///   - fileUrl: URL công khai/sạch để lưu vào DB (không có query params)
    /// </summary>
    Task<(string presignedUrl, string fileUrl)> CreatePresignedURL(
        string projectId,
        string userId,
        string fileName,
        string contentType,
        string uploadType = "attachment");
}
