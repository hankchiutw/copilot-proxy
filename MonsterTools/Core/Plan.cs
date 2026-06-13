namespace MonsterTools.Core;

public sealed class Plan
{
    public string Goal { get; set; } = string.Empty;
    public List<PlanStep> Steps { get; set; } = new();
    public bool RequiresApproval { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
}
