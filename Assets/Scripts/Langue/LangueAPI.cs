using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tools;

public class LangueAPI
{
    /// <summary> The selected language (English, French, ...) </summary>
    public static string selectedLanguage
    {
        get { return ConfigAPI.GetString("Language"); }
        set { ConfigAPI.SetString("Language", value); }
    }

    /// <summary> Get a text in the current language </summary>
    /// <param name="category">Text's Category (native, mod name...)</param>
    /// <param name="id">ID of the text</param>
    /// <param name="dontExists">Text to display if the ID doesn't exists</param>
    public static string Get(string category, string id, string dontExists) { return Get(category, id, dontExists, new string[0]); }
    /// <summary> Get a text in the current language </summary>
    /// <param name="category">Text's Category (native, mod name...)</param>
    /// <param name="id">ID of the text</param>
    /// <param name="dontExists">Text to display if the ID doesn't exists</param>
    /// <param name="arg">Text parsing arguments</param>
    public static string Get(string category, string id, string dontExists, params double[] arg) { return Get(category, id, dontExists, arg.Select(x => x.ToString()).ToArray()); }
    /// <summary> Get a text in the current language </summary>
    /// <param name="category">Text's Category (native, mod name...)</param>
    /// <param name="id">ID of the text</param>
    /// <param name="dontExists">Text to display if the ID doesn't exists</param>
    /// <param name="arg">Text parsing arguments</param>
    public static string Get(string category, string id, string dontExists, params float[] arg) { return Get(category, id, dontExists, arg.Select(x => x.ToString()).ToArray()); }
    /// <summary> Get a text in the current language </summary>
    /// <param name="category">Text's Category (native, mod name...)</param>
    /// <param name="id">ID of the text</param>
    /// <param name="dontExists">Text to display if the ID doesn't exists</param>
    /// <param name="arg">Text parsing arguments</param>
    public static string Get(string category, string id, string dontExists, params string[] arg)
    {
        string what = "|" + id + " = ";
        string c = null;
        for (int i = 0; i < 4 & c == null; i++)
        {
            string RP = "";
            string Language = "";
            if (i == 0) { RP = ConfigAPI.GetString("ressources.pack"); Language = ConfigAPI.GetString("Language"); } //First pass: Try to find the text in the user's language and in the selected RP
            else if (i == 1) { RP = "default"; Language = ConfigAPI.GetString("Language"); } //Second pass: Try to find the text in the user's language and in the default RP
            else if (i == 2) { RP = ConfigAPI.GetString("ressources.pack"); Language = "English"; } //Third pass: Try to find the text in English and in the selected RP
            else if (i == 3) { RP = "default"; Language = "English"; } //Fourth pass: Try to find the text in English and in the default RP

            string path = UnityEngine.Application.persistentDataPath + "/Ressources/" + RP + "/languages/" + category.TrimEnd('/') + "/" + Language + ".lang";
            if (File.Exists(path))
            {
                StreamReader file = new StreamReader(@path);
                string line;
                while ((line = file.ReadLine()) != null & c == null)
                {
                    if (line.Contains(what)) c = line.Substring(what.Length, line.Length - what.Length);
                }
                file.Close();
            }
        }

        if (c == null & dontExists != null) c = dontExists; //If nothing is found, return the text of the variable "dontExists"
        for (int i = 0; i < arg.Length; i++) { if (c != null) c = c.Replace("[" + i + "]", arg[i]); } //Insert the arguments in the text
        return c.Format(); //Returns the text after formatting (line breaks, tabs, ...)
    }
}
