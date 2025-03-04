using Grpc.Core;
using TaskFlow.ProjectService;
public class ProjectServiceImpl : ProjectService.ProjectServiceBase
{
    public override Task<CreateProjectRes> CreateProject(CreateProjectReq request, ServerCallContext context)
    {
        return Task.FromResult(new CreateProjectRes
        {
            Message = $"Hello, {request.Name}!"
        });
    }
}
