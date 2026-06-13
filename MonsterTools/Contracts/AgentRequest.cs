namespace MonsterTools.Contracts;

public sealed class AgentRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string Workspace { get; set; } = string.Empty;
    public string TargetModel { get; set; } = string.Empty;
    public Dictionary<string, object?> Arguments { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
