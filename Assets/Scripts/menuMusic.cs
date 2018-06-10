using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class menuMusic : MonoBehaviour {

    static bool AudioBegin = false;

    public new AudioClip audio;
    void Awake()
    {

        if (!AudioBegin)
        {
            LoadMusic(audio);
            DontDestroyOnLoad(gameObject);
            AudioBegin = true;
        }
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
#if UNITY_STANDALONE || UNITY_EDITOR  || UNITY_WSA
        string url = "file:///" + path;
#else
        string url = "file://" + path;
#endif
        StartCoroutine(StartAudio(url, timePos));
    }
    public void LoadMusic(AudioClip ac) { GetComponent<AudioSource>().clip = ac; Play(); }
    IEnumerator StartAudio(string url, float timePos)
     {
        if (url.Length > 8)
        {
            WWW audioLoader = new WWW(url);
            while (!audioLoader.isDone)
                yield return null;
            GetComponent<AudioSource>().clip = audioLoader.GetAudioClip(false, false);
            GetComponent<AudioSource>().time = timePos;
            Play();
        }
    }
}
