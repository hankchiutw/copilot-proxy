namespace MonsterTools.Services;

public sealed class ProxyStartupService
{
    public string Host { get; }
    public int Port { get; }
    public Uri MonsterToolsUri { get; }
    public Uri LmStudioUri { get; }
    public string DefaultModel { get; }
    public double Temperature { get; }
    public bool Stream { get; }

    public ProxyStartupService(IConfiguration configuration)
    {
        Host = ReadString(configuration,
            "Proxy:Host",
            "proxy:host",
            "host",
            fallback: "127.0.0.1");

        Port = ReadInt(configuration,
            "Proxy:Port",
            "proxy:port",
            "port",
            fallback: 5050);

        MonsterToolsUri = ReadUri(configuration,
            "MonsterTools:Url",
            "monsterTools:url",
            "monsterToolsEngineUrl",
            fallback: "http://127.0.0.1:5000");

        LmStudioUri = ReadUri(configuration,
            "LMStudio:Url",
            "lmStudio:url",
            "lmStudioUrl",
            fallback: "http://127.0.0.1:1234");

        DefaultModel = ReadString(configuration,
            "LMStudio:Model",
            "lmStudio:model",
            "model",
            fallback: "ibm/granite-4-h-tiny");

        Temperature = ReadDouble(configuration,
            "LMStudio:Temperature",
            "lmStudio:temperature",
            "temperature",
            fallback: 0.0);

        Stream = ReadBool(configuration,
            "LMStudio:Stream",
            "lmStudio:stream",
            "stream",
            fallback: true);
    }

    private static string ReadString(IConfiguration configuration, string key1, string key2, string key3, string fallback)
    {
        return configuration[key1]
            ?? configuration[key2]
            ?? configuration[key3]
            ?? fallback;
    }

    private static int ReadInt(IConfiguration configuration, string key1, string key2, string key3, int fallback)
    {
        return int.TryParse(configuration[key1] ?? configuration[key2] ?? configuration[key3], out var value)
            ? value
            : fallback;
    }

    private static double ReadDouble(IConfiguration configuration, string key1, string key2, string key3, double fallback)
    {
        return double.TryParse(configuration[key1] ?? configuration[key2] ?? configuration[key3], out var value)
            ? value
            : fallback;
    }

    private static bool ReadBool(IConfiguration configuration, string key1, string key2, string key3, bool fallback)
    {
        return bool.TryParse(configuration[key1] ?? configuration[key2] ?? configuration[key3], out var value)
            ? value
            : fallback;
    }

    private static Uri ReadUri(IConfiguration configuration, string key1, string key2, string key3, string fallback)
    {
        var value = configuration[key1] ?? configuration[key2] ?? configuration[key3] ?? fallback;
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            uri = new Uri(fallback, UriKind.Absolute);

        return uri;
    }
}
