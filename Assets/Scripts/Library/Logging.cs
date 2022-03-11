using System;
using System.IO;
using UnityEngine;

public static class Logging
{
    public static Action<LogType, string> NewMessage;

    private static FileInfo logFile;
    private static string logs = "";
    public static void Log(Exception e, LogType type = LogType.Exception) { Log(e.Message, type, e.StackTrace); }
    public static void Log(string logString, LogType type = LogType.Log, string stackTrace = null)
    {
        if (!IsInitialised) Debug.LogError("Not Initialised");
        UnityThread.executeInUpdate(() => NewMessage?.Invoke(type, logString));
        if (logs != "") logs += "\n\n";
        logs += "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + type + ": " + (logString + (stackTrace != null ? "\nStack Trace:\n" : "") + stackTrace).TrimEnd('\n', '\t', '\r').Replace("\n", "\n\t\t\t\t");

        UnityThread.executeInUpdate(() => File.WriteAllText(logFile.FullName, logs));
    }

    public static bool IsInitialised => logFile != null;

    public static void Initialise()
    {
        if (IsInitialised) return;
        Application.logMessageReceived += (logString, stackTrace, type) => Log(logString, type, stackTrace);

        var DT = (DateTime.Now - TimeSpan.FromSeconds(Time.realtimeSinceStartup)).ToString("yyyy-MM-dd HH-mm-ss");
        var path = Application.persistentDataPath + "/logs/";
        logFile = new FileInfo(path + DT + ".log");
        if (!logFile.Directory.Exists) Directory.CreateDirectory(logFile.DirectoryName);
        Log("The game start");
    }

    public static void DeleteLogs()
    {
        var log = File.ReadAllText(logFile.FullName);
        Directory.Delete(Application.persistentDataPath + "/logs/", true);
        Directory.CreateDirectory(Application.persistentDataPath + "/logs/");
        File.WriteAllText(logFile.FullName, log);
    }
}

