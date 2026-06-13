using MonsterTools.Core;

namespace MonsterTools.Workers.Analysis;

public sealed class ValidationWorker : ToolWorkerBase
{
    public override string Name => "validate";
    public override string Description => "Validate tool input.";

    protected override Task<ToolResult> ExecuteCoreAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        var input = request.Get<string>("input");
        if (string.IsNullOrWhiteSpace(input))
            return Task.FromResult(ToolResult.Failure("Empty input"));

        return Task.FromResult(ToolResult.Ok(input.Length > 3 ? "VALID" : "INVALID"));
    }
}
