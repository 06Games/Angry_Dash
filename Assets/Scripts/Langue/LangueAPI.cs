using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LangueAPI : MonoBehaviour
{

    public static void LangSet(string value)
    {
        ConfigAPI.SetString("Language", value);
    }

    public static string LangGet()
    {
        return ConfigAPI.GetString("Language");
    }

    static string FormatString(string st)
    {
        st = st.Replace("\\n", "\n");
        st = st.Replace("\\t", "\t");
        return st;
    }
    public static string String(string id)
    {
        string path = Application.persistentDataPath + "/Languages/" + ConfigAPI.GetString("Language") + ".lang";
        string what = "|" + id + " = ";
        return FormatString(Cherche(path, what));
    }
    public static string StringWithArgument(string id, string[] arg)
    {
        string path = Application.persistentDataPath + "/Languages/" + ConfigAPI.GetString("Language") + ".lang";
        string what = "|" + id + " = ";
        string c = Cherche(path, what);

        int i;
        for (i = 0; i < arg.Length; i++)
        {
            c = c.Replace("[" + i + "]", arg[i]);
        }
        return FormatString(c);
    }
    static string Cherche(string path, string what)
    {
        string line;
        string name = "";
        if (File.Exists(path))
        {
            System.IO.StreamReader file = new System.IO.StreamReader(@path);
            while ((line = file.ReadLine()) != null)
            {
                if (line.Contains(what))
                    name = line.Split(new string[] { what }, StringSplitOptions.None)[1];
            }
            file.Close();
        }
        return name;
    }


    static string[] URL_To_Cheker;
    static string[] LangueDispo;
    static string[] Result;
    public static IEnumerator UpdateFiles()
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Languages/"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Languages/");

        bool FilesExists = Directory.GetFiles(Application.persistentDataPath + "/Languages/").Length > 0;

        WWW www = new WWW("https://raw.githubusercontent.com/06-Games/Angry-Dash/master/Langues/index");
        yield return www;
        string[] All = www.text.Split(new string[] { "\n" }, StringSplitOptions.None);

        int lines = All.Length;
        if (string.IsNullOrEmpty(All[lines - 1]))
            lines = lines - 1;

        URL_To_Cheker = new string[lines];
        LangueDispo = new string[lines];
        Result = new string[lines];
        
        for (int i = 0; i < lines; i++)
        {
            URL_To_Cheker[i] = All[i].Split(new string[] { "[" }, StringSplitOptions.None)[1].Replace("]", "");
        }

        LangueDispo = new string[lines];
        for (int dispo = 0; dispo < lines; dispo++)
        {
            LangueDispo[dispo] = All[dispo].Split(new string[] { "[" }, StringSplitOptions.None)[0];
        }
        
        for (int j = 0; j < lines; j++)
        {
            WWW www2 = new WWW(URL_To_Cheker[j]);
            yield return www2;
            Result[j] = www2.text;

            string path  = Application.persistentDataPath + "/Languages/";
            Directory.CreateDirectory(path);
            StreamWriter writer = new StreamWriter(path + LangueDispo[j] + ".lang", false);
            writer.WriteLine(Result[j]);
            writer.Close();
        }

        if(!FilesExists)
            UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
    }
}
