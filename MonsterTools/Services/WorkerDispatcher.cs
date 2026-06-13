using MonsterTools.Core;

namespace MonsterTools.Services;

public sealed class WorkerDispatcher
{
    private readonly ToolExecutor _executor;
    private readonly ToolArgumentNormalizer _normalizer;
    private readonly WorkspaceService _workspace;

    public WorkerDispatcher(ToolExecutor executor, ToolArgumentNormalizer normalizer, WorkspaceService workspace)
    {
        _executor = executor;
        _normalizer = normalizer;
        _workspace = workspace;
    }

    public async Task<ToolResult> DispatchAsync(string toolName, Dictionary<string, object?>? args, CancellationToken cancellationToken = default)
    {
        var normalized = _normalizer.Normalize(toolName, args ?? new Dictionary<string, object?>());
        var validation = ToolValidator.Validate(toolName, normalized);
        if (!validation.Success)
            return validation;

        var request = new ToolRequest(normalized)
        {
            ToolName = toolName,
            ExecutionContextPath = normalized.TryGetValue("workspaceRoot", out var root) ? root?.ToString() ?? _workspace.WorkspaceRoot : _workspace.WorkspaceRoot
        };

        return await _executor.ExecuteAsync(toolName, request, cancellationToken);
    }
}
