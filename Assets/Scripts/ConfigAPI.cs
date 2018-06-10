using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class ConfigAPI
{
    static string configPath = Application.persistentDataPath + "/config.ini";

    public static string GetString(string d)
    {
        string id = d + " = ";

        if (File.Exists(configPath))
        {
            string[] lines = File.ReadAllLines(configPath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(id))
                    return lines[i].Replace(id, "");
            }
        }
        return null;
    }

    public static bool GetBool(string d)
    {
        bool b = false;
        try { b = bool.Parse(GetString(d)); } catch { }
        return b;
    }

    public static int GetInt(string d)
    {
        int b = 0;
        try { b = int.Parse(GetString(d)); } catch { }
        return b;
    }

    public static float GetFloat(string d)
    {
        float b = 0;
        try { b = float.Parse(GetString(d)); } catch { }
        return b;
    }

    static bool ParmExist(string d)
    {
        string id = d + " = ";

        if (!File.Exists(configPath))
            File.CreateText(configPath);

        string[] lines = File.ReadAllLines(configPath);
        int l = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(id))
                l = i;
        }

        return l > -1;
    }
    public static void SetString(string d, string p)
    {
        string id = d + " = ";

        string[] lines = new string[5] { "# Angry Dash config file", "# Edit this carefully", "# 06Games,", "# All rights reserved", "" };
        if(File.Exists(configPath))
            lines = File.ReadAllLines(configPath);
        int l = -1;
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Contains(id))
                l = i;
        }

        if (l == -1)
            lines = lines.Union(new string[1] { id + p }).ToArray();
        else lines[l] = id + p;

        File.WriteAllLines(configPath, lines);
    }

    public static void SetBool(string d, bool p) { SetString(d, p.ToString()); }
    public static void SetInt(string d, int p) { SetString(d, p.ToString()); }
    public static void SetFloat(string d, float p) { SetString(d, p.ToString()); }
}
