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
        if (!category.EndsWith("/")) category = category + "/";
        string path = languePath(category + ConfigAPI.GetString("Language") + ".lang");
        string what = "|" + id + " = ";
        string txt = FormatString(Cherche(path, what));
        if (txt == null & dontExists != null) return dontExists;
        else return txt;
    }
    public static string StringWithArgument(string category, string id, double arg) { return StringWithArgument(category, id, new string[] { arg.ToString() }); }
    public static string StringWithArgument(string category, string id, float arg) { return StringWithArgument(category, id, new string[] { arg.ToString() }); }
    public static string StringWithArgument(string category, string id, string arg) { return StringWithArgument(category, id, new string[] { arg }); }
    public static string StringWithArgument(string category, string id, string[] arg, string dontExists = null)
    {
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
        return FormatString(c);
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
    static string FormatString(string st)
    {
        if (st != null)
        {
            st = st.Replace("\\n", "\n");
            st = st.Replace("\\t", "\t");
        }
        return st;
    }

    static bool b = true;
    static bool c = true;
    public static IEnumerator SliderInfini(Slider _Slider)
    {
        while (c)
        {
            yield return new WaitForSeconds(0.01F);
            if (b)
                _Slider.value = _Slider.value + 1;
            else _Slider.value = _Slider.value - 1;

            if (_Slider.value >= 100 | _Slider.value <= 0)
                b = !b;
        }
    }

    static string[] URL_To_Cheker;
    static string[] LangueDispo;
    static string[] Result;
    public static IEnumerator UpdateFiles(Transform DownloadingFilesPanel, LangueSelector instance)
    {
        Slider _Slider = DownloadingFilesPanel.GetChild(1).GetComponent<Slider>();
        Text Etat = _Slider.transform.GetChild(3).GetComponent<Text>();
        _Slider.value = 0;
        Etat.text = "";

        instance.StartCoroutine(SliderInfini(_Slider));

        if (!Directory.Exists(Application.persistentDataPath + "/Languages/"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Languages/");

        bool FilesExists = Directory.GetFiles(Application.persistentDataPath + "/Languages/").Length > 0;

        Base.ActiveObjectStatic(DownloadingFilesPanel.gameObject);

        WWW www = new WWW("https://raw.githubusercontent.com/06-Games/Angry-Dash/master/Langues/" + Application.version + "/index");
        yield return www;
        string webResult = www.text;
        if (webResult.Contains("404: Not Found"))
        {
            Debug.LogError("Index file not found");
            www = new WWW("https://raw.githubusercontent.com/06-Games/Angry-Dash/master/Langues/" + "/index");
            yield return www;
            webResult = www.text;
            if (webResult.Contains("404: Not Found")) webResult = "";
        }
        string[] All = webResult.Split(new string[] { "\n" }, StringSplitOptions.None);

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

        c = false;
        for (int j = 0; j < lines; j++)
        {
            _Slider.value = (j + 1) / lines * 100;
            Etat.text = j + 1 + "/" + lines;

            WWW www2 = new WWW(URL_To_Cheker[j]);
            yield return www2;
            Result[j] = www2.text;
            if (!Directory.Exists(Application.persistentDataPath + "/Languages/Flags/"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Languages/Flags/");

            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            client.DownloadFileAsync(new Uri(All[j].Split(new string[] { "[" }, StringSplitOptions.None)[2].Replace("]", "")), Application.persistentDataPath + "/Languages/Flags/" + LangueDispo[j] + ".png");

            string path = Application.persistentDataPath + "/Languages/";
            Directory.CreateDirectory(path);
            StreamWriter writer = new StreamWriter(path + LangueDispo[j] + ".lang", false);
            writer.WriteLine(Result[j]);
            writer.Close();
        }

        if (!FilesExists)
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
