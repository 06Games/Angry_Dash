
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Level;
using BaseScript = Base;

public class Soundboard : MonoBehaviour
{
    SongItem[] Song;
    SongItem[] Base;


    public Editeur editor;
    public Text Music;

    public Sprite[] PlayPause;

    public GameObject DownloadPanel;
    string labelSpeed = "0 kb/s";
    string labelDownloaded = "0 MB sur 0 MB";
    public string[] ids;

    public GameObject MusicSelectorPanel;

    bool Refreshed = false;
    public void RefreshList(bool DesactiveGO = true)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/Musics/"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Musics/");

        if (InternetAPI.IsConnected())
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string Result = "";
            try { Result = client.DownloadString("https://06games.ddns.net/Projects/Games/Angry%20Dash/musics/?min=0&max=-1"); } catch { }
            string[] song = Result.Split(new string[1] { "<BR />" }, StringSplitOptions.None);

            int lenght = song.Length;
            if (string.IsNullOrEmpty(song[lenght - 1]))
                lenght = song.Length - 1;

            Base = new SongItem[lenght];

            for (int i = 0; i < lenght; i++)
            {
                Base[i] = new SongItem();
                string[] songInfo = song[i].Split(new string[1] { " ; " }, StringSplitOptions.None);
                Base[i].URL = songInfo[0];
                Base[i].Artist = HtmlCodeParse(songInfo[1].Split(new string[1] { " / " }, StringSplitOptions.None)[0]);
                Base[i].Name = HtmlCodeParse(songInfo[2]);

                WebClient c = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/musics/licences/" + WithoutSpecialCharacters(Base[i].Artist + " - " + Base[i].Name) + ".txt";
                URL = URL.Replace(" ", "%20");
                try
                {
                    Base[i].Licence = c.DownloadString(URL);
                }
                catch
                {
                    Debug.LogWarning("404 Error : File doesn't exist \n" + URL);
                    Base[i].Licence = "Error";
                }
            }

            Search(null);
            if(DesactiveGO) gameObject.SetActive(false);
            if (lenght > 0) return;
            Refreshed = true;
        }
        
        string[] sFiles = Directory.GetFiles(Application.persistentDataPath + "/Musics/");
        Base = new SongItem[sFiles.Length];
        for (int i = 0; i < sFiles.Length; i++)
        {
            TagLib.Tag TL = TagLib.File.Create(sFiles[i], "application/ogg", TagLib.ReadStyle.None).Tag;
            Base[i] = new SongItem();
            Base[i].URL = sFiles[i];
            Base[i].Artist = TL.Performers[0];
            Base[i].Name = TL.Title;
            Base[i].Licence = "";
        }
        Search(null);
        gameObject.SetActive(false);
    }

    Stopwatch sw = new Stopwatch();
    public void DownloadMusic()
    {
        if (InternetAPI.IsConnected())
        {
            editor.bloqueEchap = true;
            //editor.Selection.GetComponent<EditorSelect>().Cam.GetComponent<BaseControl>().returnScene = false;
            if (!Directory.Exists(Application.persistentDataPath + "/Musics/"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Musics/");

            string URL = Song[SongOpened].URL;

            BaseScript.ActiveObjectStatic(DownloadPanel);

            using (WebClient wc = new WebClient())
            {
                sw.Start();
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += wc_DownloadFileCompleted;

                string path = Application.persistentDataPath + "/Musics/";
                wc.DownloadFileAsync(new Uri(URL), path + Song[SongOpened].Artist + " - " + Song[SongOpened].Name);
            }
        }
    }
    
    [Obsolete("Now, ogg is supported on all platforms")]
    public static AudioType NativeFileFormat()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        return AudioType.OGGVORBIS;
#elif UNITY_ANDROID || UNITY_IOS
        return AudioType.MPEG;
#else
        return AudioType.WAV;
#endif
    }

    private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        //vitesse
        double speed = Math.Round((e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds), 1);

        UnityThread.executeInUpdate(() =>
        {
            if (NbChiffreEntier(speed) >= 0 & NbChiffreEntier(speed) < 4)
                labelSpeed = LangueAPI.StringWithArgument("native", ids[0], new string[1] { Math.Round(speed, 1).ToString() });
            else if (NbChiffreEntier(speed) >= 4)
                labelSpeed = LangueAPI.StringWithArgument("native", ids[1], new string[1] { Math.Round(speed / 1000, 1).ToString() });

            Transform tr = DownloadPanel.transform.GetChild(0);
            tr.GetComponent<Slider>().value = e.ProgressPercentage; //barre de progression
            tr.GetChild(1).GetComponent<Text>().text = labelDownloaded;
            tr.GetChild(2).GetComponent<Text>().text = labelSpeed;

            labelDownloaded = LangueAPI.StringWithArgument("native", ids[2], new string[2] { (e.BytesReceived / 1024d / 1024d).ToString("0.0"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.0") });
        });
    }
    int NbChiffreEntier(double d)
    {
        string d2 = d.ToString();
        string[] d3 = null;

        if (d2.Contains("."))
            d3 = d2.Split(new string[] { "." }, StringSplitOptions.None);
        else d3 = new string[1] { d2 };

        return d3[0].Length;
    }
    private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            print("The download has been cancelled");
            return;
        }
        if (e.Error != null) // We have an error! Retry a few times, then abort.
        {
            print("An error ocurred while trying to download file\n" + e.Error);
            return;
        }

        BaseScript.DeactiveObjectStatic(DownloadPanel);

        UnityThread.executeInUpdate(() =>
        {
            Transform go = MusicSelectorPanel.transform.GetChild(3).GetChild(0).GetChild(3);
            bool FileExists = File.Exists(Application.persistentDataPath + "/Musics/" + Song[SongOpened].Artist + " - " + Song[SongOpened].Name);
            go.GetChild(0).gameObject.SetActive(!FileExists);
            go.GetChild(1).gameObject.SetActive(FileExists);
            editor.bloqueEchap = false;
        });
    }

    public void NewStart()
    {
        int d = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("music = ") & d == -1)
                d = x;
            else if (d != -1) x = editor.component.Length;
        }
        if (d > -1 & !string.IsNullOrEmpty(editor.component[d].Replace("music = ", "")))
        {
            int p = -1;
            for (int i = 0; i < Song.Length; i++)
            {
                if ((Song[i].Artist + " - " + Song[i].Name) == editor.component[d].Replace("music = ", "") & p == -1)
                {
                    Music.text = LangueAPI.StringWithArgument("native", ids[3], new string[1] { Song[i].Name });
                    p = i;
                }
                else if (p != -1) i = editor.component.Length;
            }
            if (p == -1)
                Music.text = LangueAPI.StringWithArgument("native", ids[3], new string[1] { "Unkown Music" });
        }
        else Music.text = LangueAPI.StringWithArgument("native", ids[3], new string[1] { "No Music" });

        if (!Refreshed) RefreshList(false);
        Page(0);
    }

    int SongOpened;
    public void OpenSongInfoPanel(int panel)
    {
        Transform go = MusicSelectorPanel.transform.GetChild(3);
        SongOpened = lastFirstLine + panel;
        go.GetChild(0).GetChild(0).GetComponent<Text>().text = Song[SongOpened].Name;
        go.GetChild(0).GetChild(1).GetComponent<Text>().text = Song[SongOpened].Artist;
        go.GetChild(0).GetChild(2).GetComponent<Text>().text = Song[SongOpened].Licence;

        int d = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("music = ") & d == -1)
                d = x;
        }
        if (Song[SongOpened].Name == editor.component[d].Replace("music = ", ""))
            go.GetChild(0).GetChild(3).GetChild(1).GetChild(0).GetComponent<Text>().text = LangueAPI.String("native", ids[5]);
        else go.GetChild(0).GetChild(3).GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = LangueAPI.String("native", ids[4]);
        go.GetChild(0).GetChild(3).GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = LangueAPI.String("native", ids[6]);

        bool FileExists = File.Exists(Application.persistentDataPath + "/Musics/" + Song[SongOpened].Artist + " - " + Song[SongOpened].Name);
        go.GetChild(0).GetChild(3).GetChild(0).gameObject.SetActive(!FileExists);
        go.GetChild(0).GetChild(3).GetChild(1).gameObject.SetActive(FileExists);

        go.gameObject.SetActive(true);
    }
    public void OpenSongChangeMenu()
    {
        MusicSelectorPanel.SetActive(true);
        MusicSelectorPanel.transform.GetChild(3).gameObject.SetActive(false);
        Page(0);
    }

    int lastFirstLine = 0;
    public void ChangPage(int p) { Page(lastFirstLine + ((MusicSelectorPanel.transform.GetChild(1).childCount - 1) * p)); }
    void Page(int firstLine)
    {
        Transform ResultPanel = MusicSelectorPanel.transform.GetChild(1);
        lastFirstLine = firstLine;
        ResultPanel.GetChild(ResultPanel.childCount-1).GetChild(0).GetComponent<Button>().interactable = firstLine > 0;
        ResultPanel.GetChild(ResultPanel.childCount - 1).GetChild(1).GetComponent<Button>().interactable = firstLine + ResultPanel.childCount - 1 < Song.Length;

        for (int i = 0; i < ResultPanel.childCount - 1; i++)
        {
            Transform go = ResultPanel.GetChild(i);
            if (Song.Length > i + firstLine)
            {
                go.gameObject.SetActive(true);
                go.GetChild(0).GetComponent<Text>().text = Song[firstLine + i].Name;
                go.GetChild(1).GetComponent<Text>().text = Song[firstLine + i].Artist;
            }
            else go.gameObject.SetActive(false);
        }
    }

    bool Play = false;
    float MusicPos;
    public void PlaySong(Text txt)
    {
        if (Play) //Met en pause
        {
            if (GameObject.Find("Audio") != null)
            {
                menuMusic mm = GameObject.Find("Audio").GetComponent<menuMusic>();
                MusicPos = mm.GetComponent<AudioSource>().time;
                mm.Stop();
            }
            txt.text = LangueAPI.String("native", ids[6]);
            Play = false;
        }
        else //Fait play
        {
            if (GameObject.Find("Audio") != null)
            {
                menuMusic mm = GameObject.Find("Audio").GetComponent<menuMusic>();
                mm.LoadUnpackagedMusic(Application.persistentDataPath + "/Musics/" + Song[SongOpened].Artist + " - " + Song[SongOpened].Name, MusicPos);
            }
            txt.text = LangueAPI.String("native", ids[7]);
            Play = true;
        }

    }
    public void StopSong()
    {
        if (GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
        MusicPos = 0;
        MusicSelectorPanel.transform.GetChild(3).GetChild(0).GetChild(3).GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = LangueAPI.String("native", ids[6]);
        Play = false;
    }

    public void Choose()
    {
        int d = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("music = ") & d == -1)
                d = x;
        }
        editor.component[d] = "music = " + Song[SongOpened].Artist + " - " + Song[SongOpened].Name;

        MusicSelectorPanel.transform.GetChild(3).GetChild(0).GetChild(3).GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = LangueAPI.String("native", ids[5]);
        Music.text = LangueAPI.StringWithArgument("native", ids[3], new string[1] { Song[SongOpened].Name });
    }

    public void Exit()
    {
        if (GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();

        MusicPos = 0;
        editor.bloqueSelect = false;
        gameObject.SetActive(false);
    }

    public void Search(InputField IF)
    {
        if (IF == null)
        {
            Song = Base;
            Page(0);
        }
        else if (string.IsNullOrEmpty(IF.text) & Song != Base)
        {
            Song = Base;
            Page(0);
        }
        else if (!string.IsNullOrEmpty(IF.text))
        {
            int[] Result = new int[0];
            for (int i = 0; i < Base.Length; i++)
            {
                if (Base[i].Name.ToUpper().Contains(IF.text.ToUpper()) | Base[i].Artist.ToUpper().Contains(IF.text.ToUpper()))
                    Result = Result.Union(new int[1] { i }).ToArray();
            }

            Song = new SongItem[Result.Length];

            for (int i = 0; i < Result.Length; i++)
                Song[i] = Base[Result[i]];
            Page(0);
        }
    }

    public static string HtmlCodeParse(string s)
    {
        string s2 = s;
        try
        {
            string[] code = s.Split(new string[] { "&" }, System.StringSplitOptions.None);
            string newString = code[0];
            if (code.Length > 1)
            {
                for (int i = 1; i < code.Length; i++)
                {
                    string[] t = s.Split(new string[] { ";" }, System.StringSplitOptions.None);
                    string c = System.Text.RegularExpressions.Regex.Replace(s, "[^0-9]", "");
                    newString = newString + code[i].Replace(code[i], Char.ConvertFromUtf32(int.Parse(c))) + t[2];
                }
                s = newString;
            }

            return s;
        }
        catch { return s2; }
    }

    public static string WithoutSpecialCharacters(string s)
    {
        string formD = s.Normalize(System.Text.NormalizationForm.FormD);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        foreach (char ch in formD)
        {
            System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(ch);
            }
        }

        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }
}
