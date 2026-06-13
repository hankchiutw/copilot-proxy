using MonsterTools.Contracts;
using MonsterTools.Core;

namespace MonsterTools.Api;

public static class ExecuteAgentEndpoint
{
    public static void MapExecuteAgent(this WebApplication app)
    {
        app.MapPost("/api/agent", async (
            AgentRequest request,
            AgentLoop agent,
            CancellationToken cancellationToken) =>
        {
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine("");
            Console.WriteLine("========================================");
            Console.WriteLine("[MONSTERTOOLS AGENT HIT]");
            Console.WriteLine($"Prompt: {request.Prompt}");
            Console.WriteLine($"Workspace: {request.Workspace}");
            Console.WriteLine("========================================");
            Console.WriteLine("");

            Console.ResetColor();

            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return Results.BadRequest(new AgentResponse
                {
                    Success = false,
                    Error = "Prompt required"
                });
            }

            var result = await agent.RunAsync(
                request.Prompt,
                request.Workspace,
                cancellationToken);

            return Results.Ok(new AgentResponse
            {
                Success = result.Success,
                Output = result.Output,
                Error = result.Error
            });
        });
    }
}