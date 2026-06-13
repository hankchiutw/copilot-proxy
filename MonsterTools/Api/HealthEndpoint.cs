using MonsterTools.Services;

namespace MonsterTools.Api;

public static class HealthEndpoint
{
    public static void MapHealth(this WebApplication app)
    {
        app.MapGet("/health", async (ILMStudioService lmStudio) =>
        {
            var healthy = await lmStudio.HealthCheckAsync();
            return Results.Ok(new
            {
                service = "MonsterTools",
                lmStudio = healthy,
                timestampUtc = DateTime.UtcNow
            });
        });
    }
}
