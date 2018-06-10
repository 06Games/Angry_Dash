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
    List<string> list = new List<string> { "", "", "", "", "" };

    public string[] SongName;
    public string[] SongArtist;
    float[] MusicPos;

    public RectTransform Template;
    public Editeur editor;
    public Text Music;

    public Sprite[] PlayPause;

    public GameObject DownloadPanel;
    string labelSpeed= "0 kb/s";
    string labelDownloaded = "0 MB sur 0 MB";

    public void RefreshList () {
        //Template.position = new Vector2(0, 0);
        //Template.GetChild(0).localPosition = new Vector3(0, 0, 0);

        if (Directory.Exists(Application.persistentDataPath + "/Musics/"))
            SongPath = new string[1] { "" }.Union(Directory.GetFiles(Application.persistentDataPath + "/Musics/")).ToArray();
        else SongPath = new string[1] { "" };

        SongName = new string[SongPath.Length];
        SongArtist = new string[SongPath.Length];
        MusicPos = new float[SongPath.Length];
        for (int i = 1; i < SongPath.Length; i++)
        {
            TagLib.File file = TagLib.File.Create(SongPath[i]);
            SongName[i] = file.Tag.Title;
            SongArtist[i] = file.Tag.FirstPerformer;
        }
        SongName[0] = "No Music";

        if (editor.file == "")
            gameObject.SetActive(false);
    }

    void DownloadMusics()
    {
        if (InternetAPI.IsConnected())
        {
            gameObject.SetActive(true);

            if (Directory.Exists(Application.persistentDataPath + "/Musics/"))
                Directory.Delete(Application.persistentDataPath + "/Musics/", true);

            Directory.CreateDirectory(Application.persistentDataPath + "/Musics/");

            string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/musics/mp3";
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            client.DownloadString(new Uri(URL));
            string Result = client.DownloadString(URL).Replace("<tr><th colspan=\"5\"><hr></th></tr>", "").Replace("</table>\n</body></html>\n", "");
            string[] c = Result.Split(new string[1] { "\n" }, StringSplitOptions.None);
            int cLenght = c.Length - 13;

            string[] s = new string[cLenght];
            for (int i = 0; i < cLenght; i++)
                s[i] = c[i + 11].Split(new string[1] { "<a href=\"" }, StringSplitOptions.None)[1].Split(new string[1] { "\">" }, StringSplitOptions.None)[0];

            Base.ActiveObjectStatic(DownloadPanel);
            downloadFile(0, s, Application.persistentDataPath + "/Musics/");
        }
    }
    string[] downData = new string[3];
    Stopwatch sw = new Stopwatch();
    public void downloadFile(int actual, string[] s, string mainPath)
    {
        UnityThread.executeInUpdate(() => {
            DownloadPanel.transform.GetChild(0).GetChild(0).GetComponent<Text>().text = (actual + 1) + " / " + s.Length;
        });
        string desktopPath = mainPath + s[actual];

        string url = "https://06games.ddns.net/Projects/Games/Angry%20Dash/musics/mp3/" + s[actual];

        using (WebClient wc = new WebClient())
        {
            sw.Start();
            wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            wc.DownloadFileCompleted += wc_DownloadFileCompleted;
            wc.DownloadFileAsync(new Uri(url), desktopPath);
        }

        string newS = "";
        for (int i = 0; i < s.Length; i++)
        {
            if (i < s.Length - 1)
                newS = newS + s[i] + "\n";
            else newS = newS + s[i];
        }

        downData[0] = actual.ToString();
        downData[1] = newS;
        downData[2] = mainPath;
    }

    private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        //vitesse
        double speed = Math.Round((e.BytesReceived / 1024d / sw.Elapsed.TotalSeconds), 1);

        if (NbChiffreEntier(speed) >= 0 & NbChiffreEntier(speed) < 4)
            labelSpeed = Math.Round(speed, 1) + " Ko/s";
        else if (NbChiffreEntier(speed) >= 4)
            labelSpeed = Math.Round(speed / 1000, 1) + " Mo/s";

        UnityThread.executeInUpdate(() =>
        {
            Transform tr = DownloadPanel.transform.GetChild(0);
            tr.GetComponent<Slider>().value = e.ProgressPercentage; //barre de progression
            tr.GetChild(1).GetComponent<Text>().text = labelDownloaded;
            tr.GetChild(2).GetComponent<Text>().text = labelSpeed;
        });

        labelDownloaded = string.Format("{0} Mo sur {1} Mo", //Le nombre de MB téléchargé sur le nombre de MB total
            (e.BytesReceived / 1024d / 1024d).ToString("0.0"),
            (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.0"));
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

        UnityThread.executeInUpdate(() =>
        {
            string[] s = downData[1].Split(new string[1] { "\n" }, StringSplitOptions.None);

            if (int.Parse(downData[0]) < s.Length - 1)
                downloadFile(int.Parse(downData[0]) + 1, s, downData[2]);
            else
            {
                Base.DeactiveObjectStatic(DownloadPanel);
                RefreshList();
            }
        });
    }


    public void NewStart()
    {
        if (SongPath.Length <= 1)
            DownloadMusics();

        int d = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("music = ") & d == -1)
                d = x;
        }
        for (int i = 1; i < SongPath.Length; i++)
        {
            if (SongPath[i] == Application.persistentDataPath + "/Musics/" + editor.component[d].Replace("music = ", ""))
                Music.text = "<b> Music :</b> <i>" + SongName[i] + "</i>";
        }
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void OpenSongChangeMenu(Dropdown dd)
    {
        Template.GetChild(0).GetChild(0).GetComponent<RectTransform>().anchoredPosition = new Vector3(0, 0, 0);
        //list = new List<string>(SongPath.Length) { "No Music", "", "", "", "" };
        list = SongPath.ToList();
        for (int i = 1; i < SongPath.Length; i++)
            list[i] = SongArtist[i] + " - <i>" + SongName[i] + "</i>";

        dd.options.Clear();
        foreach (string t in list)
        {
            dd.options.Add(new Dropdown.OptionData() { text = t });
        }
        
        int d = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("music = ") & d == -1)
                d = x;
        }
        for (int i = 0; i < SongPath.Length; i++)
        {
            if (SongPath[i] == Application.persistentDataPath + "/Musics/" + editor.component[d].Replace("music = ", ""))
                dd.value = i;
        }
        
        dd.Show();

        Transform item = dd.transform.GetChild(3).GetChild(0).GetChild(0).GetChild(1);
        item.GetChild(4).gameObject.SetActive(false);
        item.GetChild(5).gameObject.SetActive(false);
        item.GetChild(6).gameObject.SetActive(false);
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

    public void OnValueChange(Dropdown dd)
    {
        int d = -1;
        for (int x = 0; x < editor.component.Length; x++)
        {
            if (editor.component[x].Contains("music = ") & d == -1)
                d = x;
        }
        editor.component[d] = "music = " + SongPath[dd.value].Replace(Application.persistentDataPath + "/Musics/", "");

        if (GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();


        Music.text = "<b> Music :</b> <i>" + SongName[dd.value] + "</i>";
    }

    public void Exit()
    {
        if (GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();

        MusicPos = new float[SongPath.Length];
        gameObject.SetActive(false);
    }
}
