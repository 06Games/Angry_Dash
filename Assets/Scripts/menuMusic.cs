using System;
using System.Collections;
using System.IO;
using System.Net;
using NVorbis;
using SoundAPI;
using Tayx.Graphy;
using Tools;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Networking;

namespace SoundAPI
{
    public static class Sound
    {
        public static AudioClip Get(string id)
        {
            var cache = Cache.Open("Ressources/sounds");
            if (cache.ValueExist(id)) return cache.Get<AudioClip>(id);
            throw new Exception("Sound isn't loaded ! Use Load() function before and wait for the end of the function");
        }
    }

    public class Load
    {
        private string id;
        private Uri url;
        public Load(string _id) { id = _id; }
        public Load(Uri uri) { url = uri; }

        private bool work = true;
        public void Cancel() { work = false; }

        public event Action<float> ReadProgressChanged;
        public event Action<AudioClip> Readable;
        public event Action<AudioClip> Complete;
        public IEnumerator Start(bool storeInCache = true)
        {
            var cache = Cache.Open("Ressources/sounds");
            AudioClip clip = null;
            var needLoad = !cache.ValueExist(id);
            if (cache.ValueExist(id))
                if (cache.Get(id) == null) needLoad = true;


            var FullyLoaded = ConfigAPI.GetBool("audio.WaitUntilFullyLoaded");
            if (needLoad)
            {
                var filePath = id;
                if (storeInCache)
                {
                    if (ConfigAPI.GetString("ressources.pack") == null)
                        ConfigAPI.SetString("ressources.pack", "default");
                    filePath = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/sounds/" + id + ".ogg";
                    if (!File.Exists(filePath)) filePath = Application.persistentDataPath + "/Ressources/default/sounds/" + id + ".ogg";
                }

                if ((id != null & File.Exists(filePath)) | url != null)
                {
#if UNITY_EDITOR || UNITY_STANDALONE
                    if (FullyLoaded) //If it is necessary to wait for the end of the loading, the native method is more advantageous
                    {
                        ReadProgressChanged?.Invoke(0F);
                        if (id != null & url == null) url = new Uri("file:///" + filePath);
                        var www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.OGGVORBIS);
                        yield return www.SendWebRequest();
                        clip = DownloadHandlerAudioClip.GetContent(www);
                        ReadProgressChanged?.Invoke(1F);
                        Readable?.Invoke(clip);
                        if (storeInCache & string.IsNullOrEmpty(id)) throw new Exception("ID must be set if you want to cache audio");
                        if (storeInCache) cache.Set(id, clip);
                    }
                    else //Otherwise, NVorbis offers faster loading chunk by chunk
                    {
#endif
                        Stream str = new MemoryStream();
                        if (id != null & url == null)
                        {
                            ReadProgressChanged?.Invoke(0F);
                            Stream stream = new FileStream(filePath, FileMode.Open);
                            stream.CopyTo(str);
                            stream.Close();
                            ReadProgressChanged?.Invoke(1F);
                        }
                        else
                        {
                            var client = new WebClient();
                            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                            var finish = false;
                            client.DownloadProgressChanged += (sender, e) =>
                            {
                                if (ReadProgressChanged != null & work)
                                    ReadProgressChanged.Invoke(e.ProgressPercentage / 100F);
                            };
                            client.DownloadDataCompleted += (sender, e) =>
                            {
                                if (!e.Cancelled)
                                {
                                    Stream stream = new MemoryStream(e.Result);
                                    stream.CopyTo(str);
                                    stream.Close();
                                    id = url.AbsoluteUri;
                                    finish = true;
                                }
                                else Logging.Log(e.Error, LogType.Error);
                            };
                            Logging.Log("Start loading music from '" + url.AbsoluteUri + "'");
                            client.DownloadDataAsync(url);
                            yield return new WaitUntil(() => finish | !work);
                            if (!work) client.CancelAsync();
                        }

                        if (work)
                        {
                            using (var vorbis = new VorbisReader(str, true))
                            {
                                var channels = vorbis.Channels; //number of channels
                                var sampleRate = vorbis.SampleRate; //sampling frequency

                                //create a buffer for reading samples
                                const double bufferLength = 0.2; //the buffer is 200ms long
                                var readBuffer = new float[(long)(channels * sampleRate * bufferLength)];

                                var sampleLength = sampleRate * vorbis.TotalTime.TotalSeconds;
                                clip = AudioClip.Create(id, (int)sampleLength, channels, sampleRate, false);
                                if (Readable != null) Readable.Invoke(clip);
                                if (storeInCache & string.IsNullOrEmpty(id)) throw new Exception("ID must be set if you want to cache audio");
                                if (storeInCache) cache.Set(id, clip);

                                int cnt; //used buffer size
                                long i = 0; //loop number
                                while ((cnt = vorbis.ReadSamples(readBuffer, 0, readBuffer.Length)) > 0)
                                {
                                    yield return null;
                                    var offset = (int)(i * sampleRate * bufferLength);
                                    clip.SetData(cnt < readBuffer.Length ? ArrayExtensions.Get(readBuffer, 0, cnt) : readBuffer, offset);
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
            Complete?.Invoke(clip);
        }

        public bool Equals(Load other) => this == other;
        public override bool Equals(object obj) => this == obj as Load;

        public static bool operator ==(Load left, Load right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.id == right.id || left.url == right.url;
        }
        public static bool operator !=(Load left, Load right) => !(left == right);
        public override int GetHashCode() => base.GetHashCode();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MenuMusic))]
public class LevelScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var myTarget = (MenuMusic)target;
        var source = myTarget.GetComponent<AudioSource>();
        EditorGUILayout.LabelField("Position", source.time + " / " + source.clip.length);
    }
}
#endif

public class MenuMusic : MonoBehaviour
{
    private static bool AudioBegin;
    public AudioMixer mixer;
    private AudioClip mainMusic;
    public bool PlayingMainMusic => GetComponent<AudioSource>().clip == mainMusic;

    private void Awake()
    {
        var parametersNames = new[] { "Master", "Musique", "FX" };
        var parametersConfigNames = new[] { "audio.master", "audio.music", "audio.fx" };
        for (var i = 0; i < parametersNames.Length; i++)
            mixer.SetFloat(parametersNames[i], ConfigAPI.GetInt(parametersConfigNames[i]));

        if (AudioBegin) return;
        try { GraphyManager.Instance.AudioListener = GetComponent<AudioListener>(); } catch { }
        StartDefault();
    }
    public void StartDefault(float timePos = 0)
    {
        if (mainMusic == null)
        {
            var load = new Load("native/main");
            load.Readable += clip =>
            {
                LoadMusic(clip, timePos);
                DontDestroyOnLoad(gameObject);
                AudioBegin = true;
                mainMusic = clip;
            };
            StartCoroutine(load.Start());
        }
        else LoadMusic(mainMusic, timePos);

    }

    private void Update()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "DontDestroyOnLoad")
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
        var load = new Load(path);
        load.Readable += clip =>
        {
            GetComponent<AudioSource>().clip = clip;
            GetComponent<AudioSource>().time = timePos;
            Play();
        };
        StartCoroutine(load.Start(false));
    }

    public void LoadMusic(string id, float timePos = 0)
    {
        if (Path.IsPathRooted(id)) throw new Exception("ID should not be a path! Use LoadUnpackagedMusic() instead");
        LoadMusic(Sound.Get(id), timePos);
    }
    public void LoadMusic(AudioClip ac, float timePos)
    {
        GetComponent<AudioSource>().clip = ac;
        GetComponent<AudioSource>().time = timePos;
        Play();
    }
}
