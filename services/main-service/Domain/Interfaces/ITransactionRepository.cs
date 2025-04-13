
using MongoDB.Driver;

namespace MainService.Domain.Interfaces;

public interface ITransactionRepo
{
    Task ExecuteAsync(Func<IClientSessionHandle, Task> action);
    Task<T> ExecuteAsync<T>(Func<IClientSessionHandle, Task<T>> action);
}