using AutoMapper;
using BaseService;
using Grpc.Core;
using MainService.Domain.Entities;
using MainService.Domain.UseCases;
using TaskFlow.CommentService;

public class CommentController : CommentService.CommentServiceBase
{
    private readonly CommentUseCase _commentUseCase;
    private readonly IMapper _mapper;

    private readonly ILogger<CommentController> _logger;

    public CommentController(
        CommentUseCase commentUseCase,
        IMapper mapper,
        ILogger<CommentController> logger)
    {
        _commentUseCase = commentUseCase;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }
    public override async Task<CreateCommentRes> CreateComment(CreateCommentReq request, ServerCallContext context)
    {
        var commentDomain = new CommentDomain
        {
            Content = request.Content,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IssueId = request.IssueId,
            UserId = request.UserId,
        };

        var createdComment = await _commentUseCase.CreateComment(commentDomain);

        var comment = _mapper.Map<CommentRes>(createdComment);
        var response = new CreateCommentRes
        {
            Data = comment,
            Message = "Create comment success",
            Status = "success",
        };

        return response;
    }
    public override async Task<ListCommentsRes> ListComments(ListCommentsReq request, ServerCallContext context)
    {
        var (comments, total) = await _commentUseCase.ListComments(new GetCommentParams
        {
            IssueId = request.IssueId,
            Page = request.Page,
            Limit = request.Limit
        });

        var totalPages = (int)Math.Ceiling((double)total / request.Limit);

        var response = new ListCommentsRes
        {
            Status = "success",
            Message = "Comments retrieved successfully",
            Pagination = new PaginationRes
            {
                TotalItems = total,
                TotalPages = totalPages,
                CurrentPage = request.Page,
                Limit = request.Limit
            }
        };

        response.Data.AddRange(_mapper.Map<IEnumerable<CommentRes>>(comments));

        return response;

    }

}
