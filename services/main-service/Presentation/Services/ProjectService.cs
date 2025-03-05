using Grpc.Core;
using TaskFlow.ProjectService;

public class ProjectServiceImpl : ProjectService.ProjectServiceBase
{
    public override Task<CreateProjectRes> CreateProject(CreateProjectReq request, ServerCallContext context)
    {
        // Create response data
        var projectResponse = new ProjectRes
        {
            Id = "12345",
            Name = request.Name,
            Description = request.Description,
            OwnerId = request.OwnerId
        };

        return Task.FromResult(new CreateProjectRes
        {
            Data = projectResponse,
        });
    }
}
