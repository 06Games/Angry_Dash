using System;
using System.Globalization;
using System.IO;
using AngryDash.Language;
using Discord;
using SoundAPI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Screen = Display.Screen;

public class BaseControl : MonoBehaviour
{
    //Echap management
    public string scene = "Home";
    public bool returnScene = true;
    [Serializable] public class OnCompleteEvent : UnityEvent { }
    [SerializeField] private OnCompleteEvent OnEchap = new OnCompleteEvent();

    public GameObject AudioPrefs;
    public GameObject DiscordPref;


    private void Awake()
    {
        UnityThread.initUnityThread();
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Logging.Initialise();
    }
    private void OnApplicationQuit()
    {
        //Delete all cache on exit
        var files = Directory.GetFiles(Application.temporaryCachePath);
        for (var i = 0; i < files.Length; i++) File.Delete(files[i]);

        Resources.UnloadUnusedAssets();
    }
    public void ChangeReturnSceneValue(bool value) { returnScene = value; }

    private void Start()
    {
        if (GameObject.Find("Audio") == null) Instantiate(AudioPrefs).name = "Audio";
        if (GameObject.Find("Discord") == null) Instantiate(DiscordPref).name = "Discord";
        if (GetComponent<AudioSource>() == null) gameObject.AddComponent<AudioSource>();

        var gos = GameObject.FindGameObjectsWithTag("Button With Click");
        for (var i = 0; i < gos.Length; i++)
            gos[i].GetComponent<Button>().onClick.AddListener(() =>
            {
                if (GetComponent<AudioSource>().clip == null)
                {
                    var load = new Load("native/click");
                    load.Complete += clip => { GetComponent<AudioSource>().clip = clip; GetComponent<AudioSource>().Play(); };
                    StartCoroutine(load.Start());
                }
                else GetComponent<AudioSource>().Play();
            });

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Home")
            _06Games.Account.Discord.NewActivity(new Activity { State = LangueAPI.Get("native", "discordHome_title", "In the home menu"), Assets = new ActivityAssets { LargeImage = "default" } });
    }

    private bool sceneChanging;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (returnScene && scene == "")
            {
                var go = transform.Find("GUI").Find("Exit Warning");
                if (go.gameObject.activeInHierarchy) go.gameObject.SetActive(false);
                else
                {
                    var btn = go.GetChild(0).GetChild(1).GetChild(0).GetComponent<Button>();
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => Base.Quit());
                    go.gameObject.SetActive(true);
                }
            }
            else if (returnScene && !sceneChanging)
            {
                SceneManager.LoadScene(scene);
                sceneChanging = true;
            }

            OnEchap.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.F11) & !VideoSettings.mobile())
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Home")
                FindObjectOfType<SettingsApplicator>().objects[0].GetComponent<VideoSettings>().FullScreen(!Screen.fullScreen);
            else
            {
                var fullscreen = !Screen.fullScreen;
                if (fullscreen) Screen.SetResolution(UnityEngine.Screen.currentResolution.width, UnityEngine.Screen.currentResolution.height, true);
                else Screen.SetResolution(1366, 768, false);
                ConfigAPI.SetBool("video.fullScreen", fullscreen);
            }
        }
    }

    public void DeleteAllCache() { Logging.DeleteLogs(); }
}
