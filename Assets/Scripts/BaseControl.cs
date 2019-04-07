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

public class BaseControl : MonoBehaviour
{
    public string scene = "Home";
    public bool returnScene = true;
    [Serializable] public class OnCompleteEvent : UnityEvent { }
    [SerializeField] private OnCompleteEvent OnEchap = new OnCompleteEvent();

    public LoadingScreenControl LSC;

    GameObject DownloadPanel;
    public Slider BigSlider;
    public Slider SmallSlider;

    public GameObject AudioPrefs;

    public GameObject DiscordPref;


    void Awake()
    {
        UnityThread.initUnityThread();
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        if (SceneManager.GetActiveScene().name == "Start")
        {
            Application.logMessageReceived += (logString, stackTrace, type) => Logging.Log(logString, type, stackTrace);
            Logging.Log("The game start", LogType.Log);
        }
    }
    private void OnApplicationQuit()
    {
        string[] files = Directory.GetFiles(Application.temporaryCachePath);
        for (int i = 0; i < files.Length; i++)
            File.Delete(files[i]);
    }
    public void ChangeReturnSceneValue(bool value) { returnScene = value; }

    public bool Controller = false;
    UnityEngine.EventSystems.EventSystem eventSystem;
    public Selectable baseSelectable;

    void Start()
    {
        if (GameObject.Find("Audio") == null) Instantiate(AudioPrefs).name = "Audio";
        if (GameObject.Find("Discord") == null) Instantiate(DiscordPref).name = "Discord";

        if (LSC == null)
            LSC = GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>();

        GameObject[] gos = GameObject.FindGameObjectsWithTag("Button With Click");
        for (int i = 0; i < gos.Length; i++)
            gos[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                SoundAPI.Load load = new SoundAPI.Load("native/click");
                load.Readable += (sender, e) => GetComponent<AudioSource>().PlayOneShot((AudioClip)e.UserState);
                StartCoroutine(load.Start());
            });

        eventSystem = GameObject.Find("EventSystem").GetComponent<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem.firstSelectedGameObject != null) baseSelectable = eventSystem.firstSelectedGameObject.GetComponent<Selectable>();

        if (SceneManager.GetActiveScene().name == "Home")
            Discord.Presence(LangueAPI.Get("native", "discordHome_title", "In the home menu"), "", new DiscordClasses.Img("default"));
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
        if (Controller & eventSystem.currentSelectedGameObject == null & baseSelectable != null) baseSelectable.Select();
        if (Controller & !newController) eventSystem.SetSelectedGameObject(null);
        Controller = newController;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (scene == "" & returnScene)
                Base.Quit(true);
            else if (!sceneChanging & returnScene)
            {
                LSC.LoadScreen(scene);
                sceneChanging = true;
            }
            
            OnEchap.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.F11) & !VideoSettings.mobile())
        {
            if (!Display.Screen.fullScreen)
                Display.Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
            else Display.Screen.SetResolution(1366, 768, false);
            ConfigAPI.SetBool("video.fullScreen", Display.Screen.fullScreen);
            
            if (SceneManager.GetActiveScene().name == "Home")
                FindObjectOfType<SettingsApplicator>().objects[0].GetComponent<VideoSettings>().FullScreen(Display.Screen.fullScreen);
        }
    }

    public void DeleteAllCache() { Logging.DeleteLogs(); }
}
