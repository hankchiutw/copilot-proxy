namespace MonsterTools.Core;

public sealed class PlanStep
{
    public string Tool { get; set; } = string.Empty;
    public Dictionary<string, object?> Args { get; set; } = new();
    public string Description { get; set; } = string.Empty;
}
