using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools.Workers.Workspace;

public sealed class WorkspaceScanWorker : ToolWorkerBase
{
    private readonly WorkspaceService _workspace;
    public override string Name => "workspace_scan";
    public override string Description => "Scan workspace and list files.";

    public WorkspaceScanWorker(WorkspaceService workspace) => _workspace = workspace;

    protected override Task<ToolResult> ExecuteCoreAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        var files = _workspace.ScanWorkspace(request.Get<string>("path") ?? "*.*").Take(500);
        return Task.FromResult(ToolResult.Ok(string.Join(Environment.NewLine, files)));
    }
}
