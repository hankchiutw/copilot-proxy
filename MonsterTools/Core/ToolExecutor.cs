namespace MonsterTools.Core;

public sealed class ToolExecutor
{
    private readonly IReadOnlyDictionary<string, IToolWorker> _workers;

    public ToolExecutor(IEnumerable<IToolWorker> workers)
    {
        _workers = workers.ToDictionary(w => w.Name, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasWorker(string toolName) => _workers.ContainsKey(toolName);

    public Task<ToolResult> ExecuteAsync(string toolName, ToolRequest request, CancellationToken cancellationToken = default)
    {
        if (!_workers.TryGetValue(toolName, out var worker))
            return Task.FromResult(ToolResult.Failure($"Tool '{toolName}' not found."));

        request.ToolName = toolName;
        return worker.ExecuteAsync(request, cancellationToken);
    }
}
