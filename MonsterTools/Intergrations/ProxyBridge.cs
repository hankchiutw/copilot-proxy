using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools.Integrations;

public sealed class ProxyBridge
{
    private readonly ToolExecutor _executor;
    private readonly ILMStudioService _lmStudio;

    public ProxyBridge(ToolExecutor executor, ILMStudioService lmStudio)
    {
        _executor = executor;
        _lmStudio = lmStudio;
    }

    public async Task<ToolResult> ExecuteToolAsync(string toolName, Dictionary<string, object?> args, CancellationToken cancellationToken = default)
    {
        return await _executor.ExecuteAsync(toolName, new ToolRequest(args), cancellationToken);
    }

    public Task<string> ExecutePromptAsync(string prompt, CancellationToken cancellationToken = default)
        => _lmStudio.AskAsync(
            "You are MonsterTools. Prefer deterministic workers. Use tools for filesystem/build/search tasks.",
            prompt,
            0.1,
            cancellationToken);
}
