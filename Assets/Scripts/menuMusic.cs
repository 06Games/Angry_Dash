using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SoundAPI
{
    public static class Sound
    {
        public static AudioClip Get(string id)
        {
            CacheManager.Cache cache = new CacheManager.Cache("Ressources/sounds");
            if (cache.ValueExist(id)) return cache.Get<AudioClip>(id);
            else throw new System.Exception("Sound isn't loaded ! Use Load() function before and wait for the end of the function");
        }
    }

    public class Load
    {
        public Load(string _id) { id = _id; }

        string id = "";
        public event Tools.BetterEventHandler Readable;
        public event Tools.BetterEventHandler Complete;
        public IEnumerator Start(bool storeInCache = true)
        {
            CacheManager.Cache cache = new CacheManager.Cache("Ressources/sounds");
            AudioClip clip = null;
            bool needLoad = !cache.ValueExist(id);
            if (cache.ValueExist(id))
                if (cache.Get(id) != null) needLoad = true;
            if (needLoad)
            {
                string filePath = id;
                if (storeInCache)
                {
                    if (ConfigAPI.GetString("ressources.pack") == null)
                        ConfigAPI.SetString("ressources.pack", "default");
                    filePath = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/sounds/" + id + ".ogg";
                    if (!System.IO.File.Exists(filePath)) filePath = Application.persistentDataPath + "/Ressources/default/sounds/" + id + ".ogg";
                }

                if (System.IO.File.Exists(filePath))
                {
#if UNITY_EDITOR || UNITY_STANDALONE
                    bool NVorbis = !ConfigAPI.GetBool("audio.WaitForComplete");
                    if (!NVorbis) //If it is necessary to wait for the end of the loading, the native method is more advantageous
                    {
                        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip("file:///" + filePath, AudioType.OGGVORBIS);
                        yield return www.SendWebRequest();
                        clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                        if (Readable != null) Readable.Invoke(null, new Tools.BetterEventArgs(clip));
                        if (storeInCache & string.IsNullOrEmpty(id)) throw new System.Exception("ID must be set if you want to cache audio");
                        else if (storeInCache) cache.Set(id, clip);
                    }
                    else //Otherwise, NVorbis offers faster loading chunk by chunk
                    {
#endif
                        System.IO.Stream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open);
                        System.IO.Stream str = new System.IO.MemoryStream();
                        stream.CopyTo(str);
                        stream.Close();
                        using (var vorbis = new NVorbis.VorbisReader(str, true))
                        {
                            var channels = vorbis.Channels; //number of channels
                            var sampleRate = vorbis.SampleRate; //sampling frequency

                            //create a buffer for reading samples
                            double bufferLength = 0.2; //the buffer is 200ms long
                            var readBuffer = new float[(long)(channels * sampleRate * bufferLength)];

                            double sampleLength = sampleRate * vorbis.TotalTime.TotalSeconds;
                            clip = AudioClip.Create(id, (int)sampleLength, channels, sampleRate, false);
                            if (Readable != null) Readable.Invoke(null, new Tools.BetterEventArgs(clip));
                            if (storeInCache & string.IsNullOrEmpty(id)) throw new System.Exception("ID must be set if you want to cache audio");
                            else if (storeInCache) cache.Set(id, clip);

                            int cnt; //used buffer size
                            long i = 0; //loop number
                            while ((cnt = vorbis.ReadSamples(readBuffer, 0, readBuffer.Length)) > 0)
                            {
                                yield return null;
                                int offset = (int)(i * sampleRate * bufferLength);
                                if (cnt < readBuffer.Length) clip.SetData(Tools.ArrayExtensions.Get(readBuffer, 0, cnt), offset);
                                else clip.SetData(readBuffer, offset);
                                i++;
                            }
                        }

#if UNITY_EDITOR || UNITY_STANDALONE
                    }
#endif
                }
            }
            if (Complete != null) Complete.Invoke(null, new Tools.BetterEventArgs(clip));
        }
    }
}

[UnityEditor.CustomEditor(typeof(menuMusic))]
public class LevelScriptEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        menuMusic myTarget = (menuMusic)target;
        AudioSource source = myTarget.GetComponent<AudioSource>();
        UnityEditor.EditorGUILayout.LabelField("Position", source.time.ToString() + " / " + source.clip.length);
    }
}

public class menuMusic : MonoBehaviour
{
    static bool AudioBegin = false;
    public UnityEngine.Audio.AudioMixer mixer;
    AudioClip MainMusic;
    public bool PlayingMainMusic { get { return GetComponent<AudioSource>().clip == MainMusic; } }

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
        if (MainMusic == null)
        {
            SoundAPI.Load load = new SoundAPI.Load("native/main");
            load.Readable += (sender, e) =>
            {
                LoadMusic((AudioClip)e.UserState, timePos);
                DontDestroyOnLoad(gameObject);
                AudioBegin = true;
                MainMusic = (AudioClip)e.UserState;
            };
            StartCoroutine(load.Start());
        }
        else LoadMusic(MainMusic, timePos);

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
    
    public void LoadUnpackagedMusic(string path, float timePos = 0)
    {
        SoundAPI.Load load = new SoundAPI.Load(path);
        load.Readable += (sender, e) =>
        {
            GetComponent<AudioSource>().clip = (AudioClip)e.UserState;
            GetComponent<AudioSource>().time = timePos;
            Play();
        };
        StartCoroutine(load.Start(false));
    }

    public void LoadMusic(string id, float timePos = 0)
    {
        if (System.IO.Path.IsPathRooted(id)) throw new System.Exception("ID should not be a path! Use LoadUnpackagedMusic() instead");
        LoadMusic(SoundAPI.Sound.Get(id), timePos);
    }
    public void LoadMusic(AudioClip ac, float timePos)
    {
        GetComponent<AudioSource>().clip = ac;
        GetComponent<AudioSource>().time = timePos;
        Play();
    }
}
