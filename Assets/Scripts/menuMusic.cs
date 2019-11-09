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
        string id = null;
        System.Uri url = null;
        public Load(string _id) { id = _id; }
        public Load(System.Uri uri) { url = uri; }

        bool work = true;
        public void Cancel() { work = false; }

        public event System.Action<float> ReadProgressChanged;
        public event System.Action<AudioClip> Readable;
        public event System.Action<AudioClip> Complete;
        public IEnumerator Start(bool storeInCache = true)
        {
            CacheManager.Cache cache = new CacheManager.Cache("Ressources/sounds");
            AudioClip clip = null;
            bool needLoad = !cache.ValueExist(id);
            if (cache.ValueExist(id))
                if (cache.Get(id) == null) needLoad = true;


            bool FullyLoaded = ConfigAPI.GetBool("audio.WaitUntilFullyLoaded");
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

                if ((id != null & System.IO.File.Exists(filePath)) | url != null)
                {
#if UNITY_EDITOR || UNITY_STANDALONE
                    if (FullyLoaded) //If it is necessary to wait for the end of the loading, the native method is more advantageous
                    {
                        if (ReadProgressChanged != null) ReadProgressChanged?.Invoke(0F);
                        if (id != null & url == null) url = new System.Uri("file:///" + filePath);
                        UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS);
                        yield return www.SendWebRequest();
                        clip = UnityEngine.Networking.DownloadHandlerAudioClip.GetContent(www);
                        if (ReadProgressChanged != null) ReadProgressChanged.Invoke(1F);
                        if (Readable != null) Readable.Invoke(clip);
                        if (storeInCache & string.IsNullOrEmpty(id)) throw new System.Exception("ID must be set if you want to cache audio");
                        else if (storeInCache) cache.Set(id, clip);
                    }
                    else //Otherwise, NVorbis offers faster loading chunk by chunk
                    {
#endif
                        System.IO.Stream str = new System.IO.MemoryStream();
                        if (id != null & url == null)
                        {
                            if (ReadProgressChanged != null) ReadProgressChanged.Invoke(0F);
                            System.IO.Stream stream = new System.IO.FileStream(filePath, System.IO.FileMode.Open);
                            stream.CopyTo(str);
                            stream.Close();
                            if (ReadProgressChanged != null) ReadProgressChanged.Invoke(1F);
                        }
                        else
                        {
                            System.Net.WebClient client = new System.Net.WebClient();
                            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                            bool finish = false;
                            client.DownloadProgressChanged += (sender, e) =>
                            {
                                if (ReadProgressChanged != null & work)
                                    ReadProgressChanged.Invoke(e.ProgressPercentage / 100F);
                            };
                            client.DownloadDataCompleted += (sender, e) =>
                            {
                                if (!e.Cancelled)
                                {
                                    System.IO.Stream stream = new System.IO.MemoryStream(e.Result);
                                    stream.CopyTo(str);
                                    stream.Close();
                                    id = url.AbsoluteUri;
                                    finish = true;
                                }
                                else Logging.Log(e.Error, LogType.Error);
                            };
                            Logging.Log("Start loading music from '" + url.AbsoluteUri + "'", LogType.Log);
                            client.DownloadDataAsync(url);
                            yield return new WaitUntil(() => finish | !work);
                            if (!work) client.CancelAsync();
                        }

                        if (work)
                        {
                            using (var vorbis = new NVorbis.VorbisReader(str, true))
                            {
                                var channels = vorbis.Channels; //number of channels
                                var sampleRate = vorbis.SampleRate; //sampling frequency

                                //create a buffer for reading samples
                                double bufferLength = 0.2; //the buffer is 200ms long
                                var readBuffer = new float[(long)(channels * sampleRate * bufferLength)];

                                double sampleLength = sampleRate * vorbis.TotalTime.TotalSeconds;
                                clip = AudioClip.Create(id, (int)sampleLength, channels, sampleRate, false);
                                if (Readable != null & !FullyLoaded) Readable.Invoke(clip);
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
                        }
#if UNITY_EDITOR || UNITY_STANDALONE
                    }
#endif
                }
            }
            else clip = Sound.Get(id);
            if (Readable != null & FullyLoaded) Readable.Invoke(clip);
            if (Complete != null) Complete.Invoke(clip);
        }

        public bool Equals(Load other) { return this == other; }
        public override bool Equals(object obj) { return this == obj as Load; }
        public static bool operator ==(Load left, Load right)
        {
            if (left is null & right is null) return true;
            else if (left is null | right is null) return false;
            else if (left.id == right.id) return true;
            else if (left.url == right.url) return true;
            else return false;
        }
        public static bool operator !=(Load left, Load right) { return !(left == right); }
        public override int GetHashCode() { return base.GetHashCode(); }
    }
}

#if UNITY_EDITOR
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
#endif

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
            load.Readable += (clip) =>
            {
                LoadMusic(clip, timePos);
                DontDestroyOnLoad(gameObject);
                AudioBegin = true;
                MainMusic = clip;
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
    public void Pause() { GetComponent<AudioSource>().Pause(); }
    public void Play() { GetComponent<AudioSource>().Play(); }

    public void LoadUnpackagedMusic(string path, float timePos = 0)
    {
        SoundAPI.Load load = new SoundAPI.Load(path);
        load.Readable += (clip) =>
        {
            GetComponent<AudioSource>().clip = clip;
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
