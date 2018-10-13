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
using System.Diagnostics;
using Debug = UnityEngine.Debug;

public class BaseControl : MonoBehaviour
{
    public string scene = "Home";
    public bool returnScene = true;
    [Serializable] public class OnCompleteEvent : UnityEvent { }
    [SerializeField] private OnCompleteEvent OnEchap;

    public LoadingScreenControl LSC;

    GameObject DownloadPanel;
    public Slider BigSlider;
    public Slider SmallSlider;

    public GameObject AudioPrefs;
    public AudioClip ButtonSoundOnClick;

    public GameObject DiscordPref;


    void Awake() { UnityThread.initUnityThread(); }
    private void OnApplicationQuit() {
        string[] files= Directory.GetFiles(Application.temporaryCachePath);
        for (int i = 0; i < files.Length; i++)
            File.Delete(files[i]);
    }
    public void ChangeReturnSceneValue(bool value) { returnScene = value; }

    public bool Controller = false;
    UnityEngine.EventSystems.EventSystem eventSystem;
    public Selectable baseSelectable;

    void Start()
    {
        if (GameObject.Find("Audio") == null)
            Instantiate(AudioPrefs).name = "Audio";

        if (GameObject.Find("Discord") == null)
            Instantiate(DiscordPref).name = "Discord";

        if (LSC == null)
            LSC = GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>();

        GameObject[] gos = GameObject.FindGameObjectsWithTag("Button With Click");
        for (int i = 0; i < gos.Length; i++)
            gos[i].GetComponent<Button>().onClick.AddListener(() => GetComponent<AudioSource>().PlayOneShot(ButtonSoundOnClick));

        eventSystem = GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>();
        if(eventSystem.firstSelectedGameObject != null) baseSelectable = eventSystem.firstSelectedGameObject.GetComponent<Selectable>();

        if (SceneManager.GetActiveScene().name == "Home")
            Discord.Presence(LangueAPI.String("discordHome_title"), "", new DiscordClasses.Img("default"));
    }

    public void SelectButton(Selectable obj)
    {
        if (Controller) obj.Select();
        baseSelectable = obj;
    }

    bool sceneChanging = false;
    void Update()
    {
        if (eventSystem == null)
        {
            eventSystem = GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>();
            baseSelectable = eventSystem.firstSelectedGameObject.GetComponent<Selectable>();
        }
        bool newController = Input.GetJoystickNames().Length > 0;
        if (newController) newController = !(Input.GetJoystickNames().Length == 1 & string.IsNullOrEmpty(Input.GetJoystickNames()[0]));
        if (newController == true & Controller == false & baseSelectable != null) baseSelectable.Select();
        if(Controller & eventSystem.currentSelectedGameObject == null & baseSelectable != null) baseSelectable.Select();
        if (Controller & !newController) eventSystem.SetSelectedGameObject(null);
        Controller = newController;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (scene == "" & returnScene)
                Base.Quit(true);
            else if (!sceneChanging & returnScene)
                LSC.LoadScreen(scene);

            if (returnScene) sceneChanging = true;
            OnEchap.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.F11) & !Settings.mobile())
        {
            if (!Screen.fullScreen)
                Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            else Screen.SetResolution(1366, 768, false);
            ConfigAPI.SetBool("window.fullscreen", Screen.fullScreen);
        }
    }

    #region Logs

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

    #endregion
}
