using MonsterTools.Core;

namespace MonsterTools.Services;

public sealed class LMStudioBridge
{
    private readonly ILMStudioService _lmStudio;

    public LMStudioBridge(ILMStudioService lmStudio)
    {
        _lmStudio = lmStudio;
    }

    public Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
        => _lmStudio.HealthCheckAsync(cancellationToken);

    public Task<string> AskAsync(
        string systemPrompt,
        string userPrompt,
        double temperature = 0.2,
        CancellationToken cancellationToken = default)
        => _lmStudio.AskAsync(systemPrompt, userPrompt, temperature, cancellationToken);

    public Task<string> QueryAsync(
        string prompt,
        double temperature = 0.2,
        CancellationToken cancellationToken = default)
        => _lmStudio.QueryModelAsync(prompt, temperature, cancellationToken);
}
