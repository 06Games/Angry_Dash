
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Soundboard : MonoBehaviour
{

    public string[] SongPath;
    public string[] SongName;
    public string[] SongArtist;
    public string[] Licences;

    string[] BasePath;
    string[] BaseName;
    string[] BaseArtist;
    string[] BaseLicence;


    public Editeur editor;
    public Text Music;

    public Sprite[] PlayPause;

    public GameObject DownloadPanel;
    string labelSpeed = "0 kb/s";
    string labelDownloaded = "0 MB sur 0 MB";
    public string[] ids;

    public GameObject MusicSelectorPanel;

    public void RefreshList()
    {
        if (InternetAPI.IsConnected())
        {
            MusicSelectorPanel.SetActive(false);

            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string Result = "";
            try { Result = client.DownloadString("https://06games.ddns.net/Projects/Games/Angry%20Dash/musics/?min=0&max=-1"); } catch { }
            string[] song = Result.Split(new string[1] { "<BR />" }, StringSplitOptions.None);

            int lenght = song.Length;
            if (string.IsNullOrEmpty(song[lenght - 1]))
                lenght = song.Length - 1;

            BasePath = new string[lenght];
            BaseName = new string[lenght];
            BaseArtist = new string[lenght];
            BaseLicence = new string[lenght];

            for (int i = 0; i < lenght; i++)
            {
                string[] songInfo = song[i].Split(new string[1] { " ; " }, StringSplitOptions.None);
                BasePath[i] = songInfo[0];
                BaseArtist[i] = songInfo[1].Split(new string[1] { " / " }, StringSplitOptions.None)[0];
                BaseName[i] = songInfo[2];

                WebClient c = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/musics/licences/" + BaseArtist[i] + " - " + BaseName[i] + ".txt";
                URL = URL.Replace(" ", "%20");
                try
                {
                    BaseLicence[i] = c.DownloadString(URL);
                }
                catch
                {
                    UnityEngine.Debug.LogWarning("404 Error : File doesn't exist \n" + URL);
                    BaseLicence[i] = "Error";
                }
            }

            Search(null);
            gameObject.SetActive(false);
            if (lenght > 0) return;
        }

            String AT = "audio/x-wav";
            if (NativeFileFormat() == AudioType.OGGVORBIS)
                AT = "application/ogg";
            else if (NativeFileFormat() == AudioType.MPEG)
                AT = "audio/mpeg";
        string[] sFiles = Directory.GetFiles(Application.persistentDataPath + "/Musics/");
        BasePath = new string[sFiles.Length];
        BaseName = new string[sFiles.Length];
        BaseArtist = new string[sFiles.Length];
        BaseLicence = new string[sFiles.Length];
        for (int i = 0; i < sFiles.Length; i++)
        {
            TagLib.Tag TL = TagLib.File.Create(sFiles[i], AT, TagLib.ReadStyle.None).Tag;
            BasePath[i] = sFiles[i];
            BaseArtist[i] = TL.Performers[0];
            BaseName[i] = TL.Title;
            BaseLicence[i] = "";
        }
        Search(null);
        gameObject.SetActive(false);
    }

    Stopwatch sw = new Stopwatch();
    public void DownloadMusic()
    {
        if (InternetAPI.IsConnected())
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Musics/"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Musics/");

            string URL = SongPath[SongOpened];

            Base.ActiveObjectStatic(DownloadPanel);

            using (WebClient wc = new WebClient())
            {
                sw.Start();
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += wc_DownloadFileCompleted;

                string path = Application.persistentDataPath + "/Musics/";
                if (NativeFileFormat() == AudioType.OGGVORBIS)
                    path = Application.temporaryCachePath + "/";
                wc.DownloadFileAsync(new Uri(URL), path + SongArtist[SongOpened] + " - " + SongName[SongOpened]);
            }
        }
    }

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
                labelSpeed = LangueAPI.StringWithArgument(ids[0], new string[1] { Math.Round(speed, 1).ToString() });
            else if (NbChiffreEntier(speed) >= 4)
                labelSpeed = LangueAPI.StringWithArgument(ids[1], new string[1] { Math.Round(speed / 1000, 1).ToString() });

            Transform tr = DownloadPanel.transform.GetChild(0);
            tr.GetComponent<Slider>().value = e.ProgressPercentage; //barre de progression
            tr.GetChild(1).GetComponent<Text>().text = labelDownloaded;
            tr.GetChild(2).GetComponent<Text>().text = labelSpeed;

            labelDownloaded = LangueAPI.StringWithArgument(ids[2], new string[2] { (e.BytesReceived / 1024d / 1024d).ToString("0.0"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.0") });
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

        if (NativeFileFormat() == AudioType.OGGVORBIS)
        {
            string fileName = SongArtist[SongOpened] + " - " + SongName[SongOpened];
            UnityThread.executeInUpdate(() =>
            {
                if (File.Exists(Application.temporaryCachePath + "/" + fileName + ".mp3"))
                    File.Delete(Application.temporaryCachePath + "/" + fileName + ".mp3");
                File.Move(Application.temporaryCachePath + "/" + fileName, Application.temporaryCachePath + "/" + fileName + ".mp3");

                if (File.Exists(Application.persistentDataPath + "/Musics/" + fileName + ".ogg"))
                    File.Delete(Application.persistentDataPath + "/Musics/" + fileName + ".ogg");

                
                FFmpeg.FFmpegAPI.Convert(Application.temporaryCachePath + "/" + fileName + ".mp3", Application.temporaryCachePath + "/" + fileName + ".ogg", new FFmpeg.handler(ConvertEnd));
            });
        }
        else
        {
            Base.DeactiveObjectStatic(DownloadPanel);

            UnityThread.executeInUpdate(() =>
            {
                Transform go = MusicSelectorPanel.transform.GetChild(3).GetChild(0).GetChild(3);
                bool FileExists = File.Exists(Application.persistentDataPath + "/Musics/" + SongArtist[SongOpened] + " - " + SongName[SongOpened]);
                go.GetChild(0).gameObject.SetActive(!FileExists);
                go.GetChild(1).gameObject.SetActive(FileExists);
            });
        }
    }

    void ConvertEnd(object sender, EventArgs e)
    { 
        UnityThread.executeInUpdate(() =>
        {
            string fileName = SongArtist[SongOpened] + " - " + SongName[SongOpened];
            if (File.Exists(Application.temporaryCachePath + "/" + fileName + ".mp3"))
                File.Delete(Application.temporaryCachePath + "/" + fileName + ".mp3");
            if (File.Exists(Application.persistentDataPath + "/Musics/" + fileName))
                File.Delete(Application.persistentDataPath + "/Musics/" + fileName);
            File.Move(Application.temporaryCachePath + "/" + fileName + ".ogg", Application.persistentDataPath + "/Musics/" + fileName);

            Base.DeactiveObjectStatic(DownloadPanel);

            Transform go = MusicSelectorPanel.transform.GetChild(3).GetChild(0).GetChild(3);
            bool FileExists = File.Exists(Application.persistentDataPath + "/Musics/" + SongArtist[SongOpened] + " - " + SongName[SongOpened]);
            go.GetChild(0).gameObject.SetActive(!FileExists);
            go.GetChild(1).gameObject.SetActive(FileExists);
        });
    }

    public void NewStart()
    {
        int d = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("music = ") & d == -1)
                d = x;
        }
        if (d > -1 & !string.IsNullOrEmpty(editor.component[d].Replace("music = ", "")))
        {
            int p = -1;
            for (int i = 0; i < SongPath.Length; i++)
            {
                if ((SongArtist[i] + " - " + SongName[i]) == editor.component[d].Replace("music = ", "") & p == -1)
                {
                    Music.text = LangueAPI.StringWithArgument(ids[3], new string[1] { SongName[i] });
                    p = i;
                }
            }
            if (p == -1)
                Music.text = LangueAPI.StringWithArgument(ids[3], new string[1] { "Unkown Music" });
        }
        else Music.text = LangueAPI.StringWithArgument(ids[3], new string[1] { "No Music" });

        Transform go = MusicSelectorPanel.transform.GetChild(1);
        for (int i = 0; i < go.childCount - 1; i++)
        {
            float BoxHeight = Screen.height - 250;
            if (go.transform.lossyScale.y != 0)
                BoxHeight = 830 / go.transform.lossyScale.y;

            go.GetChild(i).GetComponent<RectTransform>().sizeDelta = new Vector2(0, BoxHeight / 3);
            go.GetChild(i).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (BoxHeight / 6) * ((i * 2) + 1) * -1);
        }
    }

    int SongOpened;
    public void OpenSongInfoPanel(int panel)
    {
        Transform go = MusicSelectorPanel.transform.GetChild(3);
        SongOpened = lastFirstLine + panel;
        go.GetChild(0).GetChild(0).GetComponent<Text>().text = SongName[SongOpened];
        go.GetChild(0).GetChild(1).GetComponent<Text>().text = SongArtist[SongOpened];
        go.GetChild(0).GetChild(2).GetComponent<Text>().text = Licences[SongOpened];

        int d = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("music = ") & d == -1)
                d = x;
        }
        if (SongName[SongOpened] == editor.component[d].Replace("music = ", ""))
            go.GetChild(0).GetChild(3).GetChild(1).GetChild(0).GetComponent<Text>().text = LangueAPI.String(ids[5]);
        else go.GetChild(0).GetChild(3).GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = LangueAPI.String(ids[4]);
        go.GetChild(0).GetChild(3).GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = LangueAPI.String(ids[6]);

        bool FileExists = File.Exists(Application.persistentDataPath + "/Musics/" + SongArtist[SongOpened] + " - " + SongName[SongOpened]);
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
        ResultPanel.GetChild(3).GetChild(0).GetComponent<Button>().interactable = firstLine > 0;
        ResultPanel.GetChild(3).GetChild(1).GetComponent<Button>().interactable = firstLine + ResultPanel.childCount - 1 < SongName.Length;

        for (int i = 0; i < ResultPanel.childCount - 1; i++)
        {
            Transform go = ResultPanel.GetChild(i);
            if (SongName.Length > i + firstLine)
            {
                go.gameObject.SetActive(true);
                go.GetChild(0).GetComponent<Text>().text = SongName[firstLine + i];
                go.GetChild(1).GetComponent<Text>().text = SongArtist[firstLine + i];
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
            txt.text = LangueAPI.String(ids[6]);
            Play = false;
        }
        else //Fait play
        {
            if (GameObject.Find("Audio") != null)
            {
                menuMusic mm = GameObject.Find("Audio").GetComponent<menuMusic>();
                mm.LoadMusic(Application.persistentDataPath + "/Musics/" + SongArtist[SongOpened] + " - " + SongName[SongOpened], MusicPos);
            }
            txt.text = LangueAPI.String(ids[7]);
            Play = true;
        }

    }
    public void StopSong()
    {
        if (GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
        MusicPos = 0;
        MusicSelectorPanel.transform.GetChild(3).GetChild(0).GetChild(3).GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = LangueAPI.String(ids[6]);
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
        editor.component[d] = "music = " + SongArtist[SongOpened] + " - " + SongName[SongOpened];

        MusicSelectorPanel.transform.GetChild(3).GetChild(0).GetChild(3).GetChild(1).GetChild(0).GetChild(0).GetComponent<Text>().text = LangueAPI.String(ids[5]);
        Music.text = LangueAPI.StringWithArgument(ids[3], new string[1] { SongName[SongOpened] });
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
            SongPath = BasePath;
            SongName = BaseName;
            SongArtist = BaseArtist;
            Licences = BaseLicence;
            Page(0);
        }
        else if (string.IsNullOrEmpty(IF.text) & SongName != BaseName)
        {
            SongPath = BasePath;
            SongName = BaseName;
            SongArtist = BaseArtist;
            Licences = BaseLicence;
            Page(0);
        }
        else if (!string.IsNullOrEmpty(IF.text))
        {
            int[] Result = new int[0];
            for (int i = 0; i < BaseName.Length; i++)
            {
                if (BaseName[i].ToUpper().Contains(IF.text.ToUpper()) | BaseArtist[i].ToUpper().Contains(IF.text.ToUpper()))
                    Result = Result.Union(new int[1] { i }).ToArray();
            }

            SongPath = new string[Result.Length];
            SongName = new string[Result.Length];
            SongArtist = new string[Result.Length];
            Licences = new string[Result.Length];

            for (int i = 0; i < Result.Length; i++)
            {
                SongPath[i] = BasePath[Result[i]];
                SongName[i] = BaseName[Result[i]];
                SongArtist[i] = BaseArtist[Result[i]];
                Licences[i] = BaseLicence[Result[i]];
            }
            Page(0);
        }
    }
}
