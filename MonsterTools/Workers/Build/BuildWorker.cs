using System.Diagnostics;
using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools.Workers.Build;

public sealed class BuildWorker : ToolWorkerBase
{
    private readonly WorkspaceService _workspace;
    public override string Name => "build";
    public override string Description => "Run dotnet build.";

    public BuildWorker(WorkspaceService workspace) => _workspace = workspace;

    protected override async Task<ToolResult> ExecuteCoreAsync(ToolRequest request, CancellationToken cancellationToken)
    {
        var path = request.Get<string>("path") ?? _workspace.WorkspaceRoot;
        var fullPath = Directory.Exists(path) ? path : Path.GetDirectoryName(path) ?? _workspace.WorkspaceRoot;

        try
        {
            var output = await RunDotnetAsync(fullPath, "build --nologo", cancellationToken);
            return ToolResult.Ok(output);
        }
        catch (Exception ex)
        {
            return ToolResult.Failure(ex.Message);
        }
    }

    private static async Task<string> RunDotnetAsync(string workingDirectory, string arguments, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start dotnet.");
        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        return stdout + Environment.NewLine + stderr;
    }
}
