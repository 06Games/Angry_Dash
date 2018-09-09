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

    public void OpenURL(string URL) { Application.OpenURL(URL); }

    public static string GetVersion() { return Application.version; }

    public void PlayNewLevel(string LevelName)
    {
        if (File.Exists(Application.persistentDataPath + "/Level/Solo/" + LevelName + ".level"))
        {
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
            File.WriteAllLines(Application.temporaryCachePath + "/play.txt", new string[2] { Application.persistentDataPath + "/Level/Solo/" + LevelName + ".level", "Home" });
            GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>().LoadScreen("Player");
        }
        else GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>().LoadScreen("Start");
    }

    public void Gold(Text t) {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("money")))
            t.text = "0";
        else t.text = PlayerPrefs.GetString("money");
    }
}
