using System.Text.Json;
using MonsterTools.Api;
using MonsterTools.Core;
using MonsterTools.Host;
using MonsterTools.Services;
using MonsterTools.Workers.Analysis;
using MonsterTools.Workers.Build;
using MonsterTools.Workers.Git;
using MonsterTools.Workers.Workspace;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration
    .AddJsonFile("Config/proxy.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Config/lmstudio.json", optional: true, reloadOnChange: true)
    .AddJsonFile("Config/workers.json", optional: true, reloadOnChange: true);

// JSON settings for endpoints
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient("lmstudio", client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
});

// Auto-start the Node proxy gateway when MonsterTools starts.
builder.Services.AddHostedService<MonsterToolsHostService>();

// Services
builder.Services.AddSingleton<WorkspaceService>();
builder.Services.AddSingleton<BuildService>();
builder.Services.AddSingleton<ToolArgumentNormalizer>();
builder.Services.AddSingleton<ProxyStartupService>();

builder.Services.AddSingleton<ILMStudioService>(sp =>
{
    var proxyStartup = sp.GetRequiredService<ProxyStartupService>();
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient("lmstudio");
    return new LMStudioService(httpClient, proxyStartup.LmStudioUri.ToString(), proxyStartup.DefaultModel);
});

// Workers
builder.Services.AddSingleton<IToolWorker, WorkspaceScanWorker>();
builder.Services.AddSingleton<IToolWorker, ReadFileWorker>();
builder.Services.AddSingleton<IToolWorker, WriteFileWorker>();
builder.Services.AddSingleton<IToolWorker, PatchFileWorker>();
builder.Services.AddSingleton<IToolWorker, SearchWorker>();
builder.Services.AddSingleton<IToolWorker, AstWorker>();
builder.Services.AddSingleton<IToolWorker, ValidationWorker>();
builder.Services.AddSingleton<IToolWorker, BuildWorker>();
builder.Services.AddSingleton<IToolWorker, CompilerWorker>();
builder.Services.AddSingleton<IToolWorker, TestWorker>();
builder.Services.AddSingleton<IToolWorker, GitWorker>();

builder.Services.AddSingleton<ToolExecutor>();
builder.Services.AddSingleton<ToolRouter>();
builder.Services.AddSingleton<WorkerDispatcher>();
builder.Services.AddSingleton<AgentLoop>();
builder.Services.AddSingleton<ToolExecutionEngine>();
builder.Services.AddSingleton<LMStudioBridge>();
builder.Services.AddSingleton<MonsterMcpServer>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealth();
app.MapExecuteAgent();
app.MapExecuteTool();

app.Run();
