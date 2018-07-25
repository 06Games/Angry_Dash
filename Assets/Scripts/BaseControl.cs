using Microsoft.Win32;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Net;
using System.ComponentModel;
using UnityEngine.UI;
using System.Linq;

public class BaseControl : MonoBehaviour
{
    public string scene = "Home";
    public LoadingScreenControl LSC;

    GameObject DownloadPanel;
    public Slider BigSlider;
    public Slider SmallSlider;

    public GameObject AudioPrefs;
    public AudioClip ButtonSoundOnClick;
    private void OnApplicationQuit()
    {
        File.Delete(Application.temporaryCachePath + "/ac.txt");
    }

    void Start()
    {
        if (GameObject.Find("Audio") == null)
            Instantiate(AudioPrefs).name = "Audio";

        if (LSC == null)
            LSC = GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>();

        if (SceneManager.GetActiveScene().name == "Home")
            LSC.transform.GetChild(2).gameObject.SetActive(false);

        if (Directory.Exists(Application.persistentDataPath + "/Languages/"))
        {
            if (SceneManager.GetActiveScene().name == "Home")
                LSC.transform.GetChild(2).gameObject.SetActive(false);
            if (CheckTex())
                DownloadAllRequiredTex();
        }


        GameObject[] gos = GameObject.FindGameObjectsWithTag("Button With Click");
        for (int i = 0; i < gos.Length; i++)
            gos[i].GetComponent<Button>().onClick.AddListener(() => GetComponent<AudioSource>().PlayOneShot(ButtonSoundOnClick));

        if (Input.GetKeyDown(KeyCode.F11))
            Screen.fullScreen = !Screen.fullScreen;

        if (SceneManager.GetActiveScene().name == "Home")
            Discord.Presence("In the home menu", "", new DiscordClasses.Img("default"));
    }

    public static bool CheckTex()
    {
        if (SceneManager.GetActiveScene().name == "Home" & InternetAPI.IsConnected())
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Textures/1"))
                return true;
            else if (Directory.GetFiles(Application.persistentDataPath + "/Textures/1").Length == 0)
                return true;
            else if (!File.Exists(Application.persistentDataPath + "/Textures/info.ini"))
                return true;
            else if (File.ReadAllLines(Application.persistentDataPath + "/Textures/info.ini")[1] != "version=" + Application.version)
                return true;
            else return false;
        }
        else return false;
    }

    bool sceneChanging = false;
    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            if (scene == "")
            {
                Base.Quit(true);
            }
            else if(!sceneChanging)
                LSC.LoadScreen(scene);

            sceneChanging = true;
        }
    }

    void OnEnable() { Application.logMessageReceived += HandleLog; }
    void OnDisable() { Application.logMessageReceived -= HandleLog; }
    void HandleLog(string logString, string stackTrace, LogType type)
    {
#if !UNITY_EDITOR
        UnityThread.executeInUpdate(() =>
        {
            string[] trace = stackTrace.Split(new string[1] { "\n" }, StringSplitOptions.None);
            stackTrace = "";
            for (int i = 0; i < trace.Length - 1; i++)
                stackTrace = stackTrace + "\n\t\t" + trace[i];

            string DT = (DateTime.Now - TimeSpan.FromSeconds(Time.realtimeSinceStartup)).ToString("yyyy-MM-dd HH-mm-ss");
            string path = Application.persistentDataPath + "/logs/";
            string filepath = path + DT + ".log";

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            else if (!File.Exists(filepath))
                File.WriteAllText(filepath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] The game start\n\n");

            string current = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + logString + stackTrace + "\n\n";
            string line = File.ReadAllText(filepath) + current;
            File.WriteAllText(filepath, line);
        });
#endif
    }
    public static void LogNewMassage(string logString, bool inEditor = false, string stackTrace = null)
    {
        bool go = true;
#if UNITY_EDITOR
        go = inEditor;
#endif
        if (go)
        {
            UnityThread.executeInUpdate(() =>
            {
                string DT = (DateTime.Now - TimeSpan.FromSeconds(Time.realtimeSinceStartup)).ToString("yyyy-MM-dd HH-mm-ss");
                string path = Application.persistentDataPath + "/logs/";
                string filepath = path + DT + ".log";

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                if (!File.Exists(filepath))
                    File.WriteAllText(filepath, "[" + DateTime.Now.ToString("HH:mm:ss") + "] The game start\n\n");

                if (stackTrace != null)
                {
                    string[] trace = stackTrace.Split(new string[1] { "\n" }, StringSplitOptions.None);
                    stackTrace = "";
                    for (int i = 0; i < trace.Length - 1; i++)
                        stackTrace = stackTrace + "\n\t\t" + trace[i];
                }

                string current = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + logString + stackTrace + "\n\n";
                string line = File.ReadAllText(filepath) + current;
                File.WriteAllText(filepath, line);
            });
        }
    }
    public static string pathToActualLogMessage()
    {
        string DT = (DateTime.Now - TimeSpan.FromSeconds(Time.realtimeSinceStartup)).ToString("yyyy-MM-dd HH-mm-ss");
        string path = Application.persistentDataPath + "/logs/";
        return path + DT + ".log";

    }
    public void DeleteAllCache()
    {
        string DT = (DateTime.Now - TimeSpan.FromSeconds(Time.realtimeSinceStartup)).ToString("yyyy-MM-dd HH-mm-ss");
        string log = File.ReadAllText(Application.persistentDataPath + "/logs/" + DT + ".log");
        Directory.Delete(Application.persistentDataPath + "/logs/", true);
        Directory.CreateDirectory(Application.persistentDataPath + "/logs/");
        File.WriteAllText(Application.persistentDataPath + "/logs/" + DT + ".log", log);
    }

    void DownloadAllRequiredTex()
    {
        if (InternetAPI.IsConnected())
        {
            if (Directory.Exists(Application.persistentDataPath + "/Textures/"))
                Directory.Delete(Application.persistentDataPath + "/Textures/", true);

            Directory.CreateDirectory(Application.persistentDataPath + "/Textures/0");

            string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/items/" + "0";
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string Result = client.DownloadString(URL).Replace("<tr><th colspan=\"5\"><hr></th></tr>", "").Replace("</table>\n</body></html>\n", "");
            string[] c = Result.Split(new string[1] { "\n" }, StringSplitOptions.None);
            int cLenght = c.Length - 13;

            string[] s = new string[cLenght];
            for (int i = 0; i < cLenght; i++)
                s[i] = c[i + 11].Split(new string[1] { "<a href=\"" }, StringSplitOptions.None)[1].Split(new string[1] { "\">" }, StringSplitOptions.None)[0];

            DownloadPanel = LSC.transform.GetChild(2).gameObject;
            DownloadPanel.SetActive(true);
            BigSlider = DownloadPanel.transform.GetChild(1).GetComponent<Slider>();
            SmallSlider = DownloadPanel.transform.GetChild(2).GetComponent<Slider>();
            downloadFile(0, s, Application.persistentDataPath + "/Textures/", new int[2] { 0, 2 });
        }
        else LSC.transform.GetChild(3).gameObject.SetActive(true);
    }

    string[] downData = new string[5];
    public void downloadFile(int actual, string[] s, string mainPath, int[] MasterCat)
    {
        UnityThread.executeInUpdate(() =>
        {
            BigSlider.value = MasterCat[0] * 100 / MasterCat[1];
            SmallSlider.value = actual / (float)s.Length;

            BigSlider.transform.GetChild(3).GetComponent<Text>().text = LangueAPI.StringWithArgument("downloadTexType", new string[2] { MasterCat[0].ToString(), MasterCat[1].ToString() });
            SmallSlider.transform.GetChild(3).GetComponent<Text>().text = LangueAPI.StringWithArgument("downloadTexTexNumber", new string[2] { (actual + 1).ToString(), s.Length.ToString() });
        });

        string desktopPath = mainPath + MasterCat[0] + "/" + s[actual];

        string url = "https://06games.ddns.net/Projects/Games/Angry%20Dash/items/" + MasterCat[0] + "/" + s[actual];

        using (WebClient wc = new WebClient())
        {
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
        downData[3] = MasterCat[0].ToString();
        downData[4] = MasterCat[1].ToString();
    }

    private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
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
                downloadFile(int.Parse(downData[0]) + 1, s, downData[2], new int[2] { int.Parse(downData[3]), int.Parse(downData[4]) });
            else if (int.Parse(downData[3]) < int.Parse(downData[4]))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Textures/" + (int.Parse(downData[3]) + 1));

                string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/items/" + (int.Parse(downData[3]) + 1);
                WebClient client = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                client.DownloadString(new Uri(URL));
                string Result = client.DownloadString(URL).Replace("<tr><th colspan=\"5\"><hr></th></tr>", "").Replace("</table>\n</body></html>\n", "");
                string[] c = Result.Split(new string[1] { "\n" }, StringSplitOptions.None);
                int cLenght = c.Length - 13;
                string[] newS = new string[cLenght];
                for (int i = 0; i < cLenght; i++)
                    newS[i] = c[i + 11].Split(new string[1] { "<a href=\"" }, StringSplitOptions.None)[1].Split(new string[1] { "\">" }, StringSplitOptions.None)[0];

                downloadFile(0, newS, downData[2], new int[2] { int.Parse(downData[3]) + 1, int.Parse(downData[4]) });

            }
            else
            {
                Base.downloadFile(0, 1);
                Base.DeactiveObjectStatic(DownloadPanel);
                string[] lines = new string[2];
                lines[0] = "[database]";
                lines[1] = "version=" + Application.version;
                File.WriteAllLines(Application.persistentDataPath + "/Textures/info.ini", lines);
            }
        });
    }

    void Awake()
    {
        UnityThread.initUnityThread();
    }
}
