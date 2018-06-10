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
        PlayerPrefs.SetString("Langue", value);
    }

    public static string LangGet()
    {
        return PlayerPrefs.GetString("Langue");
    }

    static string FormatString(string st)
    {
        st = st.Replace("\\n", "\n");
        st = st.Replace("\\t", "\t");
        return st;
    }
    public static string String(float id)
    {
        string path = "";
#if UNITY_EDITOR
        path = @"C:\Games\06Games\06Games Launcher\Asset\Langue\" + PlayerPrefs.GetString("Langue") + ".lang";
#elif UNITY_STANDALONE
        path = Application.dataPath + "/../Asset/Langue/" + PlayerPrefs.GetString("Langue") + ".lang";
#endif
        string what = "|" + id + " = ";
        return FormatString(Cherche(path, what));
    }
    public static string StringWithArgument(float id, string[] arg)
    {
        string path = "";
#if UNITY_EDITOR
        path = @"C:\Games\06Games\06Games Launcher\Asset\Langue\" + PlayerPrefs.GetString("Langue") + ".lang";
#elif UNITY_STANDALONE
        path = Application.dataPath + "/../Asset/Langue/" + PlayerPrefs.GetString("Langue") + ".lang";
#endif
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
        WWW www = new WWW("https://raw.githubusercontent.com/06Games/06GamesLauncher/master/Asset/Langue/index");
        yield return www;
        string[] All = www.text.Split(new string[] { "\n" }, StringSplitOptions.None);

        URL_To_Cheker = new string[All.Length - 1];
        LangueDispo = new string[All.Length - 1];
        Result = new string[All.Length - 1];

        int i;
        for (i = 0; i < All.Length - 1; i++)
        {
            URL_To_Cheker[i] = All[i].Split(new string[] { "[" }, StringSplitOptions.None)[1].Replace("]", "");
        }
        int dispo;
        for (dispo = 0; dispo < All.Length - 1; dispo++)
        {
            LangueDispo[dispo] = All[dispo].Split(new string[] { "[" }, StringSplitOptions.None)[0];
        }

        int j;
        for (j = 0; j < All.Length - 1; j++)
        {
            WWW www2 = new WWW(URL_To_Cheker[j]);
            yield return www2;
            Result[j] = www2.text;

            string path = "";
#if UNITY_EDITOR
            path = @"C:\Games\06Games\06Games Launcher\Asset\Langue\";
#elif UNITY_STANDALONE
        path = Application.dataPath + "/../Asset/Langue/";
#endif
            Directory.CreateDirectory(path);
            StreamWriter writer = new StreamWriter(path + LangueDispo[j] + ".lang", false);
            writer.WriteLine(Result[j]);
            writer.Close();
        }
    }
}
