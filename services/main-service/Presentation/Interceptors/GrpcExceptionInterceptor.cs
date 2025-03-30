using Grpc.Core;
using Grpc.Core.Interceptors;

public class GrpcExceptionInterceptor : Interceptor
{
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
            throw new RpcException(new Status(StatusCode.Internal, "An unexpected error occurred: " + ex.Message));
        }
    }
}
