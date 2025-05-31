using Grpc.Core;
using MainService.Domain.Entities;

namespace MainService.Domain.UseCases;

public class CommentUseCase
{
    private readonly ICommentsRepository _commentsRepository;
    private readonly ILogger<CommentUseCase> _logger;

    public CommentUseCase(ICommentsRepository commentsRepository, ILogger<CommentUseCase> logger)
    {
        _commentsRepository = commentsRepository;
        _logger = logger;
    }

    public async Task<CommentDomain> CreateComment(CommentDomain comment)
    {
        if (string.IsNullOrEmpty(comment.UserId) || string.IsNullOrEmpty(comment.Content))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Missing required fields"));
        }

        var createdComment = await _commentsRepository.CreateComment(comment);
        return createdComment;
    }

    public async Task<CommentDomain> GetComment(string commentId)
    {
        if (string.IsNullOrEmpty(commentId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Comment ID is required"));
        }
        var comment = await _commentsRepository.GetComment(commentId);
        return comment;
    }

    public async Task<(List<CommentDomain>, int totalCount)> ListComments(GetCommentParams param)
    {
        if (param.Limit <= 0 || param.Page <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid pagination parameters"));
        }

        var (comments, totalCount) = await _commentsRepository.ListComments(param);
        return (comments, totalCount);
    }

    public async Task DeleteComment(string commentId)
    {
        if (string.IsNullOrEmpty(commentId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Comment ID is required"));
        }

        await _commentsRepository.DeleteComment(commentId);
    }
}
