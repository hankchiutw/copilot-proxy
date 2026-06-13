using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools.Workers.Workspace;

public sealed class WriteFileWorker : ToolWorkerBase
{
    private readonly WorkspaceService _workspace;
    public override string Name => "write_file";
    public override string Description => "Write file contents.";

    public WriteFileWorker(WorkspaceService workspace) => _workspace = workspace;

    protected override Task<ToolResult> ExecuteCoreAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var path = request.Get<string>("path");
            var content = request.Get<string>("content") ?? string.Empty;
            if (string.IsNullOrWhiteSpace(path))
                return Task.FromResult(ToolResult.Failure("Missing path"));

            _workspace.WriteFile(path, content);
            return Task.FromResult(ToolResult.Ok("File written"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ToolResult.Failure(ex.Message));
        }
    }
}
