using MonsterTools.Core;

namespace MonsterTools.Services;

public interface ILMStudioService
{
    Task<string> QueryModelAsync(string prompt, double temperature = 0.2, CancellationToken cancellationToken = default);
    Task<string> QueryModelHistoryAsync(List<ChatMessageContext> conversationHistory, CancellationToken cancellationToken = default);
    Task<string> AskAsync(string systemPrompt, string userPrompt, double temperature = 0.2, CancellationToken cancellationToken = default);
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
}

public sealed class ChatMessageContext
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
