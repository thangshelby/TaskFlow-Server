using MainService.Domain.Entities;

public interface ICommentsRepository
{
    Task<CommentDomain> CreateComment(CommentDomain commentDomain);
    Task<CommentDomain> GetComment(string id);
    Task<(List<CommentDomain>, int totalCount)> ListComments(GetCommentParams param);
    Task DeleteComment(string id);
}

public class GetCommentParams
{
    public string? UserId { get; set; }
    public string? IssueId { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
}
