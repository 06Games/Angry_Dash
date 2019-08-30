using AngryDash.Language;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BaseControl : MonoBehaviour
{
    //Echap management
    public string scene = "Home";
    public bool returnScene = true;
    [System.Serializable] public class OnCompleteEvent : UnityEngine.Events.UnityEvent { }
    [SerializeField] private OnCompleteEvent OnEchap = new OnCompleteEvent();

    public GameObject AudioPrefs;
    public GameObject DiscordPref;


    void Awake()
    {
        UnityThread.initUnityThread();
        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Application.logMessageReceived += (logString, stackTrace, type) => Logging.Log(logString, type, stackTrace);
            Logging.Log("The game start", LogType.Log);
        }
    }
    private void OnApplicationQuit()
    {
        //Delete all cache on exit
        string[] files = System.IO.Directory.GetFiles(Application.temporaryCachePath);
        for (int i = 0; i < files.Length; i++) System.IO.File.Delete(files[i]);
    }
    public void ChangeReturnSceneValue(bool value) { returnScene = value; }

    void Start()
    {
        if (GameObject.Find("Audio") == null) Instantiate(AudioPrefs).name = "Audio";
        if (GameObject.Find("Discord") == null) Instantiate(DiscordPref).name = "Discord";
        if (GetComponent<AudioSource>() == null) gameObject.AddComponent<AudioSource>();

        GameObject[] gos = GameObject.FindGameObjectsWithTag("Button With Click");
        for (int i = 0; i < gos.Length; i++)
            gos[i].GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
            {
                if (GetComponent<AudioSource>().clip == null)
                {
                    SoundAPI.Load load = new SoundAPI.Load("native/click");
                    load.Complete += (sender, e) => { GetComponent<AudioSource>().clip = (AudioClip)e.UserState; GetComponent<AudioSource>().Play(); };
                    StartCoroutine(load.Start());
                }
                else GetComponent<AudioSource>().Play();
            });

        if (SceneManager.GetActiveScene().name == "Home")
            DiscordController.Presence(LangueAPI.Get("native", "discordHome_title", "In the home menu"), "", new DiscordClasses.Img("default"));
    }

    bool sceneChanging = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (scene == "" & returnScene) Base.Quit(true);
            else if (!sceneChanging & returnScene)
            {
                LoadingScreenControl.GetLSC().LoadScreen(scene);
                sceneChanging = true;
            }

            OnEchap.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.F11) & !VideoSettings.mobile())
        {
            if (SceneManager.GetActiveScene().name == "Home")
                FindObjectOfType<SettingsApplicator>().objects[0].GetComponent<VideoSettings>().FullScreen(!Display.Screen.fullScreen);
            else
            {
                bool fullscreen = !Display.Screen.fullScreen;
                if (fullscreen) Display.Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
                else Display.Screen.SetResolution(1366, 768, false);
                ConfigAPI.SetBool("video.fullScreen", fullscreen);
            }
        }
    }

    public void DeleteAllCache() { Logging.DeleteLogs(); }
}
