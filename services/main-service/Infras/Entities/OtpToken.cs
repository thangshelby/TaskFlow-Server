using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MainService.Infras.Entities;

public class OtpToken
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("user_id")]
    public string? UserId { get; set; }

    [BsonElement("otp_hash")]
    public string OtpHash { get; set; } = default!;

    [BsonElement("salt")]
    public string Salt { get; set; } = default!;

    [BsonElement("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [BsonElement("attempt_count")]
    public int AttemptCount { get; set; } = 0;

    [BsonElement("max_attempts")]
    public int MaxAttempts { get; set; } = 5;

    [BsonElement("resend_count")]
    public int ResendCount { get; set; } = 0;

    [BsonElement("resend_window_start")]
    public DateTime ResendWindowStart { get; set; } = DateTime.UtcNow;

    [BsonElement("can_resend_after")]
    public DateTime CanResendAfter { get; set; } = DateTime.UtcNow;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("status")]
    public string Status { get; set; } = "active";
}
