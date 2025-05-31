using AutoMapper;
using MainService.Domain.Entities;
using MainService.Infras.Entities;
using MongoDB.Driver;

namespace MainService.Infras.Repositories;

public class CommentsRepository : ICommentsRepository
{
    private readonly ILogger<CommentsRepository> _logger;
    private readonly IMongoCollection<Comment> _comments;
    private readonly IMapper _mapper;

    public CommentsRepository(MongoDbService mongoDbService, IMapper mapper, ILogger<CommentsRepository> logger)
    {
        _logger = logger;
        _mapper = mapper;
        var database = mongoDbService.Database;
        _comments = database.GetCollection<Comment>("comments");
    }

    public async Task<CommentDomain> CreateComment(CommentDomain commentDomain)
    {
        var entity = _mapper.Map<Comment>(commentDomain);
        await _comments.InsertOneAsync(entity);
        commentDomain.Id = entity.Id;
        return commentDomain;
    }

    public async Task<CommentDomain> GetComment(string id)
    {
        var entity = await _comments.Find(c => c.Id == id).FirstOrDefaultAsync();
        if (entity == null)
            throw new Exception("Comment not found");

        return _mapper.Map<CommentDomain>(entity);
    }

    public async Task<(List<CommentDomain>, int totalCount)> ListComments(GetCommentParams param)
    {
        var filter = Builders<Comment>.Filter.Empty;

        if (!string.IsNullOrEmpty(param.UserId))
        {
            filter = Builders<Comment>.Filter.Eq(c => c.UserId, param.UserId);
        }
        if (!string.IsNullOrEmpty(param.IssueId))
        {
            filter = Builders<Comment>.Filter.Eq(c => c.IssueId, param.IssueId);
        }

        var totalCount = (int)await _comments.CountDocumentsAsync(filter);

        var entities = await _comments
            .Find(filter)
            .SortByDescending(c => c.CreatedAt)
            .Skip((param.Page - 1) * param.Limit)
            .Limit(param.Limit)
            .ToListAsync();

        var domains = _mapper.Map<List<CommentDomain>>(entities);
        return (domains, totalCount);
    }

    public async Task DeleteComment(string id)
    {
        var result = await _comments.DeleteOneAsync(c => c.Id == id);
        if (result.DeletedCount == 0)
            throw new Exception("Comment not found");
    }
}
