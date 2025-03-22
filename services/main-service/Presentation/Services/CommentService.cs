using Grpc.Core;
using TaskFlow.CommentService;
public class CommentServiceImpl : CommentService.CommentServiceBase
{
    public override Task<CreateCommentRes> CreateComment(CreateCommentReq request, ServerCallContext context)
    {
        return Task.FromResult(new CreateCommentRes
        {
            Message = $"Hello, {request.Name}!"
        });
    }
}
