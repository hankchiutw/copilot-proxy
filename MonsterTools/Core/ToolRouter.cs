namespace MonsterTools.Core;

public sealed class ToolRouter
{
    private readonly ToolExecutor _executor;

    public ToolRouter(ToolExecutor executor)
    {
        _executor = executor;
    }

    public bool HasWorker(string toolName) => _executor.HasWorker(toolName);

    public Task<ToolResult> ExecuteAsync(string toolName, ToolRequest request, CancellationToken cancellationToken = default)
        => _executor.ExecuteAsync(toolName, request, cancellationToken);
}
