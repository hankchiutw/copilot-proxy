using MonsterTools.Contracts;
using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools.Api;

public static class ExecuteToolEndpoint
{
    public static void MapExecuteTool(this WebApplication app)
    {
        app.MapPost("/api/tool/execute", async (
            ToolExecuteRequest request,
            WorkerDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(request.Tool, request.Args, cancellationToken);

            return Results.Ok(new ToolExecuteResponse
            {
                Success = result.Success,
                Output = result.Output,
                Error = result.Error
            });
        });

        app.MapPost("/api/tools/execute", async (
            ToolExecuteRequest request,
            WorkerDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(request.Tool, request.Args, cancellationToken);

            return Results.Ok(new ToolExecuteResponse
            {
                Success = result.Success,
                Output = result.Output,
                Error = result.Error
            });
        });

        app.MapPost("/api/tool", async (
            ToolExecuteRequest request,
            WorkerDispatcher dispatcher,
            CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.DispatchAsync(request.Tool, request.Args, cancellationToken);

            return Results.Ok(new ToolExecuteResponse
            {
                Success = result.Success,
                Output = result.Output,
                Error = result.Error
            });
        });
    }
}
