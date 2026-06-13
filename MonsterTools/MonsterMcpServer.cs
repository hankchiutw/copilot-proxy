using System.Text.Json;
using MonsterTools.Core;
using MonsterTools.Services;

namespace MonsterTools;

public sealed class MonsterMcpServer
{
    private readonly AgentLoop _agent;

    public MonsterMcpServer(AgentLoop agent) => _agent = agent;

    public string Handle(string inputJson)
    {
        try
        {
            var request = JsonSerializer.Deserialize<McpEnvelope>(inputJson);
            if (request is null || string.IsNullOrWhiteSpace(request.prompt))
                return JsonSerializer.Serialize(new { error = "Invalid request" });

            var result = _agent.RunAsync(request.prompt, request.workspace ?? string.Empty).GetAwaiter().GetResult();
            return JsonSerializer.Serialize(new { success = result.Success, output = result.Output, error = result.Error });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    private sealed class McpEnvelope
    {
        public string prompt { get; set; } = string.Empty;
        public string? workspace { get; set; }
    }
}
