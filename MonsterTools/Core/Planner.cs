namespace MonsterTools.Core;

public sealed class Planner
{
    public Plan CreatePlan(string prompt)
    {
        var text = prompt.ToLowerInvariant();
        var plan = new Plan { Goal = prompt };

        if (text.Contains("review") || text.Contains("audit"))
        {
            plan.Steps.Add(new PlanStep { Tool = "workspace_scan", Description = "Scan workspace" });
            plan.Steps.Add(new PlanStep { Tool = "build", Description = "Build solution" });
            return plan;
        }

        if (text.Contains("compile") || text.Contains("build"))
        {
            plan.Steps.Add(new PlanStep { Tool = "build", Description = "Build project" });
            return plan;
        }

        if (text.Contains("file") || text.Contains("read") || text.Contains("write"))
        {
            plan.Steps.Add(new PlanStep { Tool = "workspace_scan", Description = "Inspect workspace" });
        }

        return plan;
    }
}
