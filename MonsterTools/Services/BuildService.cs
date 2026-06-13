using System.Diagnostics;
using System.Text;

namespace MonsterTools.Services;

public sealed class BuildService
{
    public async Task<string> ExecuteAsync(string executable, string arguments, string workingDirectory, CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException($"Failed to start {executable}");

        var stdout = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderr = await process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return stdout + Environment.NewLine + stderr;
    }

    public Task<string> BuildAsync(string path, CancellationToken cancellationToken = default) =>
        ExecuteAsync("dotnet", "build --nologo", path, cancellationToken);

    public Task<string> TestAsync(string path, CancellationToken cancellationToken = default) =>
        ExecuteAsync("dotnet", "test --nologo", path, cancellationToken);
}
