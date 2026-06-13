using System.Text.Json;

namespace MonsterTools.Core;

public sealed class MonsterMcpServer
{
    private readonly AgentLoop _agent;

    public MonsterMcpServer(AgentLoop agent)
    {
        _agent = agent;
    }

    public void Run()
    {
        while (true)
        {
            var line = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var request = JsonSerializer.Deserialize<McpRequest>(line);

                if (request is null || string.IsNullOrWhiteSpace(request.tool))
                    continue;

                var result = _agent
                    .RunToolDirectAsync(request.tool, request.args)
                    .GetAwaiter()
                    .GetResult();

                Console.WriteLine(
                    JsonSerializer.Serialize(
                        new McpResponse
                        {
                            id = request.id,
                            success = result.Success,
                            result = result.Success
                                ? result.Output
                                : result.Error
                        }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    JsonSerializer.Serialize(
                        new
                        {
                            error = ex.Message
                        }));
            }
        }
    }
}