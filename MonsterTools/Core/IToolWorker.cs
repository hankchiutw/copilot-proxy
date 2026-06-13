namespace MonsterTools.Core;

public interface IToolWorker
{
    string Name { get; }
    string Description { get; }
    Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken = default);
}
