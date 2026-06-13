using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting;
using MonsterTools.Core;
using MonsterTools.Integrations;
using MonsterTools.Services;
using MonsterTools.Workers.Analysis;
using MonsterTools.Workers.Build;
using MonsterTools.Workers.Git;
using MonsterTools.Workers.Workspace;

namespace MonsterTools.Host;

public sealed class MonsterToolsHostService : IHostedService
{
    private Process? _proxyProcess;
    private readonly ProxyStartupService _proxyConfig;

    public MonsterToolsHostService(ProxyStartupService proxyConfig)
    {
        _proxyConfig = proxyConfig;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("[Host Engine] Registering MonsterTools Core Engine Layer...");

        // Eagerly construct the engine graph so miswired dependencies fail fast on startup.
        var buildService = new BuildService();
        var workspace = new WorkspaceService();
        var workers = new IToolWorker[]
        {
            new WorkspaceScanWorker(workspace),
            new ReadFileWorker(workspace),
            new WriteFileWorker(workspace),
            new PatchFileWorker(workspace),
            new SearchWorker(workspace),
            new AstWorker(workspace),
            new ValidationWorker(),
            new BuildWorker(workspace),
            new CompilerWorker(buildService, workspace),
            new TestWorker(buildService, workspace),
            new GitWorker(workspace),
            new FileSystemWorkers(workspace)
        };

        var executor = new ToolExecutor(workers);
        var dispatcher = new WorkerDispatcher(executor, new ToolArgumentNormalizer(), workspace);
        var httpClient = new HttpClient();
        var lmStudio = new LMStudioService(httpClient, _proxyConfig.LmStudioUri.ToString(), _proxyConfig.DefaultModel);
        var normalizer = new ToolArgumentNormalizer();

        var agent = new AgentLoop(lmStudio, new ToolRouter(executor), normalizer);
        var bridge = new ProxyBridge(executor, lmStudio);

        _ = bridge;
        _ = dispatcher;
        _ = agent;

        StartNodeProxyGateway();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        StopNodeProxyGateway();
        return Task.CompletedTask;
    }

    private void StartNodeProxyGateway()
    {
        try
        {
            if (IsPortListening(_proxyConfig.Host, _proxyConfig.Port))
            {
                Console.WriteLine($"[Host Engine] Copilot Proxy already listening on {_proxyConfig.Host}:{_proxyConfig.Port}. Skipping auto-start.");
                return;
            }

            string baseDirectory = AppContext.BaseDirectory;
            string proxyPath = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", "..", "..", "copilot-proxy-tools"));

            if (!Directory.Exists(proxyPath))
            {
                Console.WriteLine($"[Host Warning] Node.js Gateway folder path not resolved: {proxyPath}");
                return;
            }

            Console.WriteLine("[Host Engine] Auto-starting Copilot Proxy Gateway via powershell constraint context...");

            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var startInfo = new ProcessStartInfo
            {
                FileName = isWindows ? "powershell.exe" : "npm",
                Arguments = isWindows ? "-NoProfile -ExecutionPolicy Bypass -Command \"npm run dev\"" : "run dev",
                WorkingDirectory = proxyPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            _proxyProcess = new Process { StartInfo = startInfo };

            _proxyProcess.OutputDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    Console.WriteLine($"[Proxy Node Log] {args.Data}");
            };

            _proxyProcess.ErrorDataReceived += (sender, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[Proxy Node Error] {args.Data}");
                    Console.ResetColor();
                }
            };

            _proxyProcess.Start();
            _proxyProcess.BeginOutputReadLine();
            _proxyProcess.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Host Error] Background gateway failed to load: {ex.Message}");
        }
    }

    private void StopNodeProxyGateway()
    {
        if (_proxyProcess != null && !_proxyProcess.HasExited)
        {
            try
            {
                Console.WriteLine("[Host Engine] Tearing down background Node.js processes cleanly...");
                _proxyProcess.Kill(true);
                _proxyProcess.Dispose();
                _proxyProcess = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Host Warning] Resource cleanup exception: {ex.Message}");
            }
        }
    }

    private static bool IsPortListening(string host, int port)
    {
        try
        {
            using var client = new TcpClient();
            var connectTask = client.ConnectAsync(host, port);
            if (!connectTask.Wait(TimeSpan.FromMilliseconds(300)))
                return false;

            return client.Connected;
        }
        catch
        {
            return false;
        }
    }
}
