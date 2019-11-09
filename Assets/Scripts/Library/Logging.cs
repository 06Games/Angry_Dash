using System.IO;
using UnityEngine;

public static class Logging
{
    public static System.Action<LogType, string> NewMessage;

    public static void Log(System.Exception e, LogType type = LogType.Exception) { Log(e.Message, type, e.StackTrace); }
    public static void Log(string logString, LogType type = LogType.Log, string stackTrace = null)
    {
        NewMessage?.Invoke(type, logString);
        UnityThread.executeInUpdate(() =>
        {
            if (stackTrace != null)
            {
                string[] trace = stackTrace.Split(new string[1] { "\n" }, System.StringSplitOptions.None);
                stackTrace = "";
                for (int i = 0; i < trace.Length - 1; i++)
                    stackTrace = stackTrace + "\n\t\t" + trace[i];
            }

            FileInfo file = new FileInfo(pathToLogFile);

            if (!file.Directory.Exists)
                Directory.CreateDirectory(file.DirectoryName);

            string current = "[" + System.DateTime.Now.ToString("HH:mm:ss") + "] " + //date
            type.ToString() + ": " + //type
            logString + stackTrace + "\n\n";  //Message + trace
            if (file.Exists) current = File.ReadAllText(file.FullName) + current;
            File.WriteAllText(file.FullName, current);
        });
    }
    public static string pathToLogFile
    {
        get
        {
            string DT = (System.DateTime.Now - System.TimeSpan.FromSeconds(Time.realtimeSinceStartup)).ToString("yyyy-MM-dd HH-mm-ss");
            string path = Application.persistentDataPath + "/logs/";
            return path + DT + ".log";
        }
    }

    public static void DeleteLogs()
    {
        string DT = (System.DateTime.Now - System.TimeSpan.FromSeconds(Time.realtimeSinceStartup)).ToString("yyyy-MM-dd HH-mm-ss");
        string log = File.ReadAllText(Application.persistentDataPath + "/logs/" + DT + ".log");
        Directory.Delete(Application.persistentDataPath + "/logs/", true);
        Directory.CreateDirectory(Application.persistentDataPath + "/logs/");
        File.WriteAllText(Application.persistentDataPath + "/logs/" + DT + ".log", log);
    }
}

