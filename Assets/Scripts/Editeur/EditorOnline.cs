using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class EditorOnline : MonoBehaviour
{

    public Transform rZone;
    public Transform levelPanel;
    public Transform CommentPanel;
    public Transform InfoPanel;
    public Transform loadingPanel;
    public Editeur Editor;
    public string[] ids;
    string[] files;

    void Start()
    {
        levelPanel.gameObject.SetActive(false);
        Search(null);
    }

    string[] level = new string[0];
    string[] description = new string[0];
    string[] music = new string[0];
    string[] author = new string[0];
    string[] id = new string[0];
    int page = 0;
    public void Search(InputField IF)
    {
        files = new string[0];
        if (InternetAPI.IsConnected())
        {
            rZone.gameObject.SetActive(true);
            string key = "";
            if (IF != null) key = IF.text;
            if (string.IsNullOrEmpty(key)) key = "/";
            string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/index.php?key=" + key;
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            string Result = "";
            loadingPanel.gameObject.SetActive(true);
            loadingPanel.GetChild(0).GetComponent<GIF_Reader>().StartGIF();
            try { Result = client.DownloadString(URL.Replace(" ", "%20")); } catch { }
            loadingPanel.gameObject.SetActive(false);
            loadingPanel.GetChild(0).GetComponent<GIF_Reader>().StopGIF();

            files = Result.Split(new string[1] { "<BR />" }, StringSplitOptions.None);
        }

        int item = 0;
        int length = files.Length;
        if (length > 0)
        {
            if (files[length - 1] == "")
                length = length - 1;
        }

        level = new string[length];
        description = new string[length];
        music = new string[length];
        author = new string[length];
        id = new string[length];
        page = 0;

        for (int i = 0; i < length; i++)
        {
            if (!string.IsNullOrEmpty(files[i]))
            {
                string[] file = new string[1] { files[i] };
                if (files[i].Contains(" ; "))
                    file = files[i].Split(new string[1] { " ; " }, System.StringSplitOptions.None);

                for (int l = 0; l < file.Length; l++)
                {
                    string[] line = file[l].Split(new string[1] { " = " }, System.StringSplitOptions.None);
                    if (line[0] == "level")
                        level[item] = file[l].Replace("level = ", "");
                    else if (line[0] == "author")
                        author[item] = file[l].Replace("author = ", "");
                    else if (line[0] == "description")
                        description[item] = file[l].Replace("description = ", "");
                    else if (line[0] == "music")
                        music[item] = file[l].Replace("music = ", "");
                    else if (line[0] == "publicID")
                        id[item] = file[l].Replace("publicID = ", "");
                }
                if (string.IsNullOrEmpty(level[item])) { level[item] = ""; }
                if (string.IsNullOrEmpty(description[item])) { description[item] = ""; }
                if (string.IsNullOrEmpty(music[item])) { music[item] = ""; }
                if (string.IsNullOrEmpty(author[item])) { author[item] = LangueAPI.String(ids[0]); }
                if (string.IsNullOrEmpty(id[item])) { id[item] = LangueAPI.String(ids[1]); }


                item = item + 1;
            }
        }

        rZone.GetChild(4).GetChild(0).GetComponent<Button>().interactable = page > 0;
        rZone.GetChild(4).GetChild(1).GetComponent<Button>().interactable = page + 4 < level.Length;
        for (int i = page; i < page + 4; i++)
        {
            Transform go = rZone.GetChild(i - page);

            if (i < item)
            {
                go.gameObject.SetActive(true);
                go.GetChild(0).GetComponent<Text>().text = level[i];
                go.GetChild(1).GetComponent<Text>().text = LangueAPI.StringWithArgument(ids[2], new string[1] { author[i] });
                string lvlID = ""; if (id[i].Length > 10) lvlID = id[i].Substring(0, 10); else lvlID = id[i];
                go.GetChild(2).GetComponent<Text>().text = LangueAPI.StringWithArgument(ids[3], new string[1] { lvlID });
            }
            else go.gameObject.SetActive(false);
        }
    }
    public void Page(int c)
    {
        page = page + (4 * c);
        rZone.GetChild(4).GetChild(0).GetComponent<Button>().interactable = page > 0;
        rZone.GetChild(4).GetChild(1).GetComponent<Button>().interactable = page + 4 < level.Length;

        for (int i = page; i < page + 4; i++)
        {
            Transform go = rZone.GetChild(i - page);
            if (i < level.Length)
            {
                go.gameObject.SetActive(true);
                go.GetChild(0).GetComponent<Text>().text = level[i];
                go.GetChild(1).GetComponent<Text>().text = LangueAPI.StringWithArgument(ids[2], new string[1] { author[i] });
                string lvlID = ""; if (id[i].Length > 10) lvlID = id[i].Substring(0, 10); else lvlID = id[i];
                go.GetChild(2).GetComponent<Text>().text = LangueAPI.StringWithArgument(ids[3], new string[1] { lvlID });
            }
            else go.gameObject.SetActive(false);
        }
    }

    int actual = 0;
    public void OpenLevelMenu(int l)
    {
        actual = page + l;
        levelPanel.GetChild(1).GetChild(0).GetComponent<Text>().text = level[actual];
        levelPanel.GetChild(1).GetChild(2).GetComponent<Text>().text = author[actual];

        if (!string.IsNullOrEmpty(music[actual]))
        {
            string[] a = music[actual].Split(new string[1] { " - " }, System.StringSplitOptions.None);
            levelPanel.GetChild(4).GetChild(0).GetComponent<Text>().text = LangueAPI.StringWithArgument(ids[4], new string[2] { a[1], a[0] });
            levelPanel.GetChild(4).GetChild(1).gameObject.SetActive(!File.Exists(Application.persistentDataPath + "/Musics/" + music[actual]));
            levelPanel.GetChild(4).gameObject.SetActive(true);
        }
        else levelPanel.GetChild(4).gameObject.SetActive(false);

        InfoPanel.GetChild(2).GetChild(0).GetComponent<Text>().text = description[actual];
        GetComponent<CreatorManager>().ChangArray(1);
    }

    Stopwatch sw = new Stopwatch();
    public void DownloadMusic()
    {
        if (InternetAPI.IsConnected())
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Musics/"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Musics/");

            string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/musics/mp3/" + Soundboard.WithoutSpecialCharacters(music[actual]).Replace(" ", "%20") + ".mp3";
            UnityEngine.Debug.LogWarning(URL);

            levelPanel.GetChild(4).GetChild(1).gameObject.SetActive(false);
            levelPanel.GetChild(4).GetChild(2).gameObject.SetActive(true);

            using (WebClient wc = new WebClient())
            {
                sw.Start();
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += wc_DownloadFileCompleted;

                string path = Application.persistentDataPath + "/Musics/";
                if (Soundboard.NativeFileFormat() == AudioType.OGGVORBIS)
                    path = Application.temporaryCachePath + "/";
                wc.DownloadFileAsync(new Uri(URL), path + Soundboard.WithoutSpecialCharacters(music[actual]));
            }
        }
    }
    private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        /*//vitesse
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
        });*/
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
        else if (Soundboard.NativeFileFormat() == AudioType.OGGVORBIS)
        {
            string fileName = Soundboard.WithoutSpecialCharacters(music[actual]);
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
            UnityThread.executeInUpdate(() =>
            {
                levelPanel.GetChild(4).GetChild(1).gameObject.SetActive(false);
                levelPanel.GetChild(4).GetChild(2).gameObject.SetActive(false);
            });
        }
    }
    void ConvertEnd(object sender, EventArgs e)
    {
        UnityThread.executeInUpdate(() =>
        {
            string fileName = Soundboard.WithoutSpecialCharacters(music[actual]);
            if (File.Exists(Application.temporaryCachePath + "/" + fileName + ".mp3"))
                File.Delete(Application.temporaryCachePath + "/" + fileName + ".mp3");
            if (File.Exists(Application.persistentDataPath + "/Musics/" + fileName))
                File.Delete(Application.persistentDataPath + "/Musics/" + fileName);
            File.Move(Application.temporaryCachePath + "/" + fileName + ".ogg", Application.persistentDataPath + "/Musics/" + music[actual]);

            levelPanel.GetChild(4).GetChild(1).gameObject.SetActive(false);
            levelPanel.GetChild(4).GetChild(2).gameObject.SetActive(false);
        });
    }

    public void PlayLevel()
    {
        if (InternetAPI.IsConnected())
        {
            string pID = "_" + id[actual];
            if (pID == "_" + LangueAPI.String(ids[1])) pID = "";

            string url = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/files/" + author[actual] + "/" + level[actual] + pID + ".level";
            string path = Application.temporaryCachePath + "/" + level[actual] + ".level";
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };


            loadingPanel.gameObject.SetActive(true);
            loadingPanel.GetChild(0).GetComponent<GIF_Reader>().StartGIF();
            string file = "";
            try { file = client.DownloadString(url.Replace(" ", "%20")); } catch { }
            loadingPanel.gameObject.SetActive(false);
            loadingPanel.GetChild(0).GetComponent<GIF_Reader>().StopGIF();

            if (File.Exists(path))
                File.Delete(path);

            if (!string.IsNullOrEmpty(file))
            {
                File.WriteAllText(path, file);
                File.WriteAllLines(Application.temporaryCachePath + "/play.txt", new string[2] { path, "Editor/Online" });
                GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>().LoadScreen("Player");
            }
        }
    }
}
