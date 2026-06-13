using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools.Workers.Workspace;

public sealed class PatchFileWorker : ToolWorkerBase
{
    private readonly WorkspaceService _workspace;
    public override string Name => "patch_file";
    public override string Description => "Patch text in a file.";

    public PatchFileWorker(WorkspaceService workspace) => _workspace = workspace;

    protected override Task<ToolResult> ExecuteCoreAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var path = request.Get<string>("path");
            var find = request.Get<string>("find");
            var replace = request.Get<string>("replace") ?? string.Empty;

            if (string.IsNullOrWhiteSpace(path))
                return Task.FromResult(ToolResult.Failure("Missing path"));
            if (string.IsNullOrWhiteSpace(find))
                return Task.FromResult(ToolResult.Failure("Missing find"));

            _workspace.PatchFile(path, find, replace);
            return Task.FromResult(ToolResult.Ok("File patched"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(ToolResult.Failure(ex.Message));
        }
    }
}
