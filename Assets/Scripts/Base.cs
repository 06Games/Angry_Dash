using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

public class Base : MonoBehaviour {

    [Serializable] public class OnCompleteEvent : UnityEvent { }
    [SerializeField] private OnCompleteEvent OnUpdate;

    private void Update() { OnUpdate.Invoke(); }

    public void Scene(string levelName) { UnityEngine.SceneManagement.SceneManager.LoadScene(levelName); /* Charge la scene */ }

    public void Quit()
    {
        Quit(true);
    }
    public static void Quit(bool forceEditor = true)
    {
#if UNITY_EDITOR
        if (forceEditor)
            UnityEditor.EditorApplication.isPlaying = false;
        else Debug.Log("The game as been close");
#else
            Debug.Log("The game as been close");
            Application.Quit();
#endif
    }

    public void ActiveObject(GameObject go) { go.SetActive(true); }
    public static void ActiveObjectStatic(GameObject go) { UnityThread.executeInUpdate(() => go.SetActive(true)); }

    public void DeactiveObject(GameObject go) { go.SetActive(false); }
    public static void DeactiveObjectStatic(GameObject go) { UnityThread.executeInUpdate(() => go.SetActive(false)); }

    public static string GetVersion() { return Application.version; }

    public void PlayNewLevel(string LevelName)
    {
        if (File.Exists(Application.persistentDataPath + "/Level/Solo/" + LevelName + ".level"))
        {
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
            File.WriteAllLines(Application.temporaryCachePath + "/play.txt", new string[2] { Application.persistentDataPath + "/Level/Solo/" + LevelName + ".level", "Home" });
            GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>().LoadScreen("Player");
        }
        else
        {
            string url = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/solo/";
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string result = client.DownloadString(new Uri(url));
            string[] results = result.Split(new string[] { "\n"}, StringSplitOptions.None);
            int lenght = results.Length - 14;
            if(string.IsNullOrEmpty(results[results.Length-1]))
                lenght = results.Length - 15;

            downloadFile(0, lenght);
        }
    }
    

    static int[] downData = new int[2];
    public static void downloadFile(int actual, int max)
    {
        if (InternetAPI.IsConnected())
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Level/Solo/"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Level/Solo/");

            UnityThread.executeInUpdate(() =>
            {
                GameObject go = GameObject.Find("LoadingScreen").transform.GetChild(1).gameObject;
                go.SetActive(true);
                go.transform.GetChild(0).GetComponent<Slider>().value = (actual + 1) / (max + 1);
                go.transform.GetChild(2).GetComponent<Text>().text = actual + "/" + max;
            });

            string desktopPath = Application.persistentDataPath + "/Level/Solo/Level " + (actual + 1) + ".level";

            string url = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/solo/" + (actual + 1) + ".level";

            using (WebClient wc = new WebClient())
            {
                wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new Uri(url), desktopPath);
            }
            downData[0] = actual;
            downData[1] = max;
        }
    }
    
    private static void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
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
            if (downData[0] < downData[1] - 1)
                downloadFile(downData[0] + 1, downData[1]);
            else DeactiveObjectStatic(GameObject.Find("LoadingScreen").transform.GetChild(1).gameObject);
        });
    }

    public void Gold(Text t) {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("money")))
            t.text = "0";
        else t.text = PlayerPrefs.GetString("money");
    }
}
