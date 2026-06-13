namespace MonsterTools.Core;

public abstract class ToolWorkerBase : IToolWorker
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    public Task<ToolResult> ExecuteAsync(ToolRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            return ExecuteCoreAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            return Task.FromResult(ToolResult.Failure(ex.Message));
        }
    }

    protected abstract Task<ToolResult> ExecuteCoreAsync(ToolRequest request, CancellationToken cancellationToken);
}
