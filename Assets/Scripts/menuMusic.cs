using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuMusic : MonoBehaviour
{
    static bool AudioBegin = false;

    public AudioClip audioClip;

    public UnityEngine.Audio.AudioMixer mixer;

    void Awake()
    {
        string[] parametersNames = new string[3] { "Master", "Musique", "FX" };
        string[] parametersConfigNames = new string[] { "audio.master", "audio.music", "audio.fx" };
        for (int i = 0; i < parametersNames.Length; i++)
            mixer.SetFloat(parametersNames[i], ConfigAPI.GetInt(parametersConfigNames[i]));

        if (!AudioBegin)
        {
            try { Tayx.Graphy.GraphyManager.Instance.AudioListener = GetComponent<AudioListener>(); } catch { }
            StartDefault();
        }
    }
    public void StartDefault(float timePos = 0)
    {
        LoadMusic(audioClip, timePos);
        DontDestroyOnLoad(gameObject);
        AudioBegin = true;
    }
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "DontDestroyOnLoad")
        {
            GetComponent<AudioSource>().Stop();
            AudioBegin = false;
        }
        if (GameObject.FindGameObjectsWithTag("Audio").Length > 1)
            Destroy(GameObject.FindGameObjectsWithTag("Audio")[1]);
    }

    public void Stop() { GetComponent<AudioSource>().Stop(); }
    public void Play() { GetComponent<AudioSource>().Play(); }

    public void LoadMusic(string path, float timePos = 0)
    {
#if UNITY_STANDALONE || UNITY_EDITOR || UNITY_WSA
        string url = "file:///" + path;
#else
        string url = "file://" + path;
#endif
        StartCoroutine(StartAudio(url, timePos));
    }
    public void LoadMusic(AudioClip ac, float timePos)
    {
        GetComponent<AudioSource>().clip = ac;
        GetComponent<AudioSource>().time = timePos;
        Play();
    }
    IEnumerator StartAudio(string url, float timePos)
    {
        if (url.Length > 8)
        {
            UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, Soundboard.NativeFileFormat());
            yield return www.SendWebRequest();

            if (www.isNetworkError) Debug.LogError(www.error);
            else
            {
                GetComponent<AudioSource>().clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                GetComponent<AudioSource>().time = timePos;
                Play();
            }
        }
    }
}
