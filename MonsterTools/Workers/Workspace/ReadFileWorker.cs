using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools.Workers.Workspace;

public sealed class ReadFileWorker : ToolWorkerBase
{
    private readonly WorkspaceService _workspace;
    public override string Name => "read_file";
    public override string Description => "Read file contents.";

    public ReadFileWorker(WorkspaceService workspace) => _workspace = workspace;

    protected override Task<ToolResult> ExecuteCoreAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var path = request.Get<string>("path");
            if (string.IsNullOrWhiteSpace(path))
                return Task.FromResult(ToolResult.Failure("Missing path"));

            return Task.FromResult(ToolResult.Ok(_workspace.ReadFile(path)));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ToolResult.Failure(ex.Message));
        }
    }
}
