using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LangueAPI : MonoBehaviour
{
    public static string languePath(string id)
    {
        if (ConfigAPI.GetString("ressources.pack") == null)
            ConfigAPI.SetString("ressources.pack", "default");
        string path = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/languages/" + id;
        if (Directory.Exists(path)) return path;
        else return Application.persistentDataPath + "/Ressources/default/languages/" + id;
    }

    public static void LangSet(string value) { ConfigAPI.SetString("Language", value); }
    public static string LangGet() { return ConfigAPI.GetString("Language"); }

    /// <summary>
    /// Get a text translation
    /// </summary>
    /// <param name="category">Text's Category (native, mod name...)</param>
    /// <param name="id">ID of the text</param>
    /// <param name="dontExists">Text to display if the ID doesn't exists</param>
    /// <returns></returns>
    public static string String(string category, string id, string dontExists = null)
    {
        if (!category.Contains("native")) Debug.LogError("Only native is supported !");

        if (!category.EndsWith("/")) category = category + "/";
        string path = languePath(category + ConfigAPI.GetString("Language") + ".lang");
        string what = "|" + id + " = ";
        string txt = (new Tools.String(Cherche(path, what))).Format.GetString;
        if (txt == null & dontExists != null) return dontExists;
        else return txt;
    }
    public static string StringWithArgument(string category, string id, double arg, string dontExists = null) { return StringWithArgument(category, id, new string[] { arg.ToString() }, dontExists); }
    public static string StringWithArgument(string category, string id, float arg, string dontExists = null) { return StringWithArgument(category, id, new string[] { arg.ToString() }, dontExists); }
    public static string StringWithArgument(string category, string id, string arg, string dontExists = null) { return StringWithArgument(category, id, new string[] { arg }, dontExists); }
    public static string StringWithArgument(string category, string id, string[] arg, string dontExists = null)
    {
        if (!category.Contains("native")) Debug.LogError("Only native is supported !");

        if (!category.EndsWith("/")) category = category + "/";
        string path = languePath(category + ConfigAPI.GetString("Language") + ".lang");
        string what = "|" + id + " = ";
        string c = Cherche(path, what);

        if (c == null & dontExists != null) c = dontExists;
        int i;
        for (i = 0; i < arg.Length; i++)
        {
            if (c != null) c = c.Replace("[" + i + "]", arg[i]);
        }
        return (new Tools.String(c)).Format.GetString;
    }
    static string Cherche(string path, string what)
    {
        string line;
        string name = null;
        if (File.Exists(path))
        {
            StreamReader file = new  StreamReader(@path);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains(what))
                    name = line.Split(new string[] { what }, StringSplitOptions.None)[1];
            }
            file.Close();
        }
        return name;
    }
}
