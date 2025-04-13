using MainService.Domain.Interfaces;
using MainService.Infras;
using MongoDB.Driver;

public class MongoTransactionRepo : ITransactionRepo
{
    private readonly MongoDbService _mongoDbService;

    public MongoTransactionRepo(MongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }

    public async Task ExecuteAsync(Func<IClientSessionHandle, Task> action)
    {
        using var session = await _mongoDbService.MongoClient.StartSessionAsync();
        session.StartTransaction();

        try
        {
            await action(session);
            await session.CommitTransactionAsync();
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<IClientSessionHandle, Task<T>> action)
    {
        using var session = await _mongoDbService.MongoClient.StartSessionAsync();
        session.StartTransaction();

        try
        {
            var result = await action(session);
            await session.CommitTransactionAsync();
            return result;
        }
        catch
        {
            await session.AbortTransactionAsync();
            throw;
        }
    }
}
