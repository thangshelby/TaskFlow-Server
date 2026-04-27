using Grpc.Core;
using Grpc.Core.Interceptors;

public class GrpcExceptionInterceptor : Interceptor
{
    private readonly ILogger<GrpcExceptionInterceptor> _logger;

    public GrpcExceptionInterceptor(ILogger<GrpcExceptionInterceptor> logger)
    {
        _logger = logger;
    }

    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {

            return await continuation(request, context);
        }
        catch (RpcException)
        {
            // ✅ If it's already an RpcException, rethrow it without modification
            throw;
        }
        catch (ValidationException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
        }
        catch (UserAlreadyExistsException ex)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, ex.Message));
        }
        catch (DatabaseException ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, "Database error: " + ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in gRPC method {Method}", context.Method);
            throw new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred: " + ex.Message));
        }
    }
}
