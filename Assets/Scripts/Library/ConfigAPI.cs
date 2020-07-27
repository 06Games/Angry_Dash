using System.IO;
using System.Linq;
using UnityEngine;

public class ConfigAPI
{
    static string configPath = Application.persistentDataPath + "/config.ini";

    public static string GetString(string d)
    {
        var id = d + " = ";
        if (File.Exists(configPath))
            foreach (var line in File.ReadAllLines(configPath))
                if (line.Contains(id)) return line.Remove(0, id.Length);
        return null;
    }
    public static bool GetBool(string d) => bool.TryParse(GetString(d), out var b) ? b : false;
    public static int GetInt(string d) => int.TryParse(GetString(d), out var b) ? b : 0;
    public static float GetFloat(string d) => float.TryParse(GetString(d), out var b) ? b : 0;

    public static bool Exists(string d)
    {
        var id = d + " = ";
        if (File.Exists(configPath))
            foreach (var line in File.ReadAllLines(configPath))
                if (line.Contains(id)) return true;
        return false;
    }
    public static void SetString(string d, string p)
    {
        string id = d + " = ";

        var lines = new[] { "# Angry Dash config file", "# Edit this carefully", "# 06Games,", "# All rights reserved", "" };
        if (File.Exists(configPath))
        {
            lines = File.ReadAllLines(configPath);
            for (int i = 0; i < lines.Length; i++)
                if (lines[i].Contains(id)) { lines[i] = id + p; Write(); return; }
        }

        lines = lines.Union(new string[1] { id + p }).ToArray();
        Write();

        void Write() => File.WriteAllLines(configPath, lines);
    }
    public static void SetBool(string d, bool p) => SetString(d, p.ToString());
    public static void SetInt(string d, int p) => SetString(d, p.ToString());
    public static void SetFloat(string d, float p) => SetString(d, p.ToString());
}
