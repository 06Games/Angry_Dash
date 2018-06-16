using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class Soundboard : MonoBehaviour {

    public string[] SongPath;
    public string[] SongName;
    public string[] SongArtist;
    float[] MusicPos;
    
    public Editeur editor;
    public Text Music;

    public Sprite[] PlayPause;

    public GameObject DownloadPanel;
    string labelSpeed= "0 kb/s";
    string labelDownloaded = "0 MB sur 0 MB";
    public string[] ids;

    public GameObject MusicSelectorPanel;

    public void RefreshList () {
        MusicSelectorPanel.SetActive(false);

        WebClient client = new WebClient();
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string Result = client.DownloadString("https://06games.ddns.net/Projects/Games/Angry%20Dash/musics/?min=0&max=-1");
        string[] song = Result.Split(new string[1] { "<BR />" }, StringSplitOptions.None);

        int lenght = song.Length;
        if(string.IsNullOrEmpty(song[lenght-1]))
            lenght = song.Length-1;

        SongPath = new string[lenght];
        SongName = new string[lenght];
        SongArtist = new string[lenght];
        MusicPos = new float[lenght];

        for (int i = 0; i < lenght; i++)
        {
            string[] songInfo = song[i].Split(new string[1] { " ; " }, StringSplitOptions.None);
            SongPath[i] = songInfo[0];
            SongArtist[i] = songInfo[1].Split(new string[1] { " / " }, StringSplitOptions.None)[0];
            SongName[i] = songInfo[2];
        }
        
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
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            URL = SongPath[SongOpened].Replace("/mp3/", "/ogg/").Replace(".mp3", ".ogg");
#endif

            Base.ActiveObjectStatic(DownloadPanel);

            using (WebClient wc = new WebClient())
            {
                sw.Start();
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new Uri(URL), Application.persistentDataPath + "/Musics/"+SongName[SongOpened]);
            }
        }
    }
    public static string FileFormat()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        return ".ogg";
#elif UNITY_ANDROID || UNITY_IOS
        return ".mp3";
#else
        return ".wav";
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

        labelDownloaded = LangueAPI.StringWithArgument(ids[2], new string[2]{ (e.BytesReceived / 1024d / 1024d).ToString("0.0"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.0")});
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
        Base.DeactiveObjectStatic(DownloadPanel);

        UnityThread.executeInUpdate(() =>
        {
            Transform go = MusicSelectorPanel.transform.GetChild(3);
            bool FileExists = File.Exists(Application.persistentDataPath + "/Musics/" + SongName[SongOpened]);
            go.GetChild(0).GetChild(2).gameObject.SetActive(!FileExists);
            go.GetChild(0).GetChild(3).gameObject.SetActive(FileExists);
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
        for (int i = 0; i < SongPath.Length; i++)
        {
            if (SongName[i] == editor.component[d].Replace("music = ", ""))
                Music.text = LangueAPI.StringWithArgument(ids[3], new string[1] { SongName[i] });
        }
    }

    int SongOpened;
	public void OpenSongInfoPanel(int panel)
    {
        Transform go = MusicSelectorPanel.transform.GetChild(3);
        SongOpened = lastFirstLine + panel;
        go.GetChild(0).GetChild(0).GetComponent<Text>().text = SongName[SongOpened];
        go.GetChild(0).GetChild(1).GetComponent<Text>().text = SongArtist[SongOpened];
        bool FileExists = File.Exists(Application.persistentDataPath + "/Musics/" + SongName[SongOpened]);
        go.GetChild(0).GetChild(2).gameObject.SetActive(!FileExists);
        go.GetChild(0).GetChild(3).gameObject.SetActive(FileExists);
        go.gameObject.SetActive(true);
    }

    public void OpenSongChangeMenu()
    {
        MusicSelectorPanel.SetActive(true);
        Page(0);
    }

    public int lastFirstLine = 0;
    public void ChangPage(int p) { Page(lastFirstLine + ((MusicSelectorPanel.transform.GetChild(1).childCount-1) * p)); }

    void Page(int firstLine)
    {
        lastFirstLine = firstLine;

        Transform ResultPanel = MusicSelectorPanel.transform.GetChild(1);
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
    
    public void PlaySong(Transform Item)
    {
        Text OptionText = Item.GetChild(3).GetComponent<Text>();
        Image Im = Item.GetChild(4).GetComponent<Image>();

        int index = -1;
        for (int i = 0; i < SongName.Length; i++)
        {
            if (index == -1 & OptionText.text.Split(new string[1] { " - <i>" }, System.StringSplitOptions.None)[1].Replace("</i>", "") == SongName[i])
                index = i;
        }

        if (Im.sprite == PlayPause[1]) //Met en pause
        {
            if (GameObject.Find("Audio") != null)
            {
                menuMusic mm = GameObject.Find("Audio").GetComponent<menuMusic>();
                MusicPos[index] = mm.GetComponent<AudioSource>().time;
                mm.Stop();
            }
            Im.sprite = PlayPause[0];
        }
        else //Fait play
        {
            if (GameObject.Find("Audio") != null)
            {
                menuMusic mm = GameObject.Find("Audio").GetComponent<menuMusic>();
                mm.LoadMusic(SongPath[index], MusicPos[index]);
            }

            Transform Go = GameObject.Find("Dropdown List").transform.GetChild(0).GetChild(0);
            for (int i = 1; i < Go.childCount; i++)
                Go.GetChild(i).GetChild(4).GetComponent<Image>().sprite = PlayPause[0];
            Im.sprite = PlayPause[1];
        }

    }
    public void StopSong(Text OptionText) {
        if (GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();

        int index = -1;
        for (int i = 0; i < SongName.Length; i++)
        {
            if (index == -1 & OptionText.text.Split(new string[1] { " - <i>" }, System.StringSplitOptions.None)[1].Replace("</i>", "") == SongName[i])
                index = i;
        }
        MusicPos[index] = 0;

        Transform Go = GameObject.Find("Dropdown List").transform.GetChild(0).GetChild(0);
        for (int i = 1; i < Go.childCount; i++)
            Go.GetChild(i).GetChild(4).GetComponent<Image>().sprite = PlayPause[0];
    }

    public void Choose()
    {
        int d = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("music = ") & d == -1)
                d = x;
        }
        editor.component[d] = "music = " + SongName[SongOpened];

        if (GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();


        Music.text = LangueAPI.StringWithArgument(ids[3], new string[1] { SongName[SongOpened] });
    }

    public void Exit()
    {
        if (GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();

        MusicPos = new float[SongPath.Length];
        gameObject.SetActive(false);
    }
}
