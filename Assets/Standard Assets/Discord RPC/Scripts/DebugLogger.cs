using DiscordRPC.Logging;
using UnityEngine;

public class DebugLogger : DiscordRPC.Logging.ILogger
{
    public LogLevel Level { get; set; }

    public void Info(string message, params object[] args)
    {
        if (Level != LogLevel.Info) return;
        Debug.Log("[DRPC] " + string.Format(message, args));
    }

    public void Warning(string message, params object[] args)
    {
        if (Level != LogLevel.Info && Level != LogLevel.Warning) return;
        Debug.LogWarning("[DRPC] " + string.Format(message, args));
    }

    public void Error(string message, params object[] args)
    {
        if (Level != LogLevel.Info && Level != LogLevel.Warning && Level != LogLevel.Error) return;
        Debug.LogError("[DRPC] " + string.Format(message, args));
    }
}
