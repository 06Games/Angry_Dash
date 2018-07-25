using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.InteropServices;

public class menuMusic : MonoBehaviour
{

    static bool AudioBegin = false;

    public AudioClip audioClip;
    void Awake()
    {
        if (!AudioBegin)
            StartDefault();
    }
    public void StartDefault()
    {
        LoadMusic(audioClip);
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
        if (Soundboard.NativeFileFormat() != "mp3")
            url = path;
        StartCoroutine(StartAudio(url, timePos));
    }
    public void LoadMusic(AudioClip ac) { GetComponent<AudioSource>().clip = ac; Play(); }
    IEnumerator StartAudio(string url, float timePos)
    {
        if (url.Length > 8)
        {
            if (Soundboard.NativeFileFormat() == "mp3")
            {
                WWW audioLoader = new WWW(url);
                while (!audioLoader.isDone)
                    yield return null;
                GetComponent<AudioSource>().clip = audioLoader.GetAudioClip(false, false, AudioType.MPEG);
            }
            else GetComponent<AudioSource>().clip = MP3Import(url);
            GetComponent<AudioSource>().time = timePos;
            Play();
        }
    }

    #region MP3 Reader For Standalone
    //IntPtr errPtr;
    IntPtr rate;
    IntPtr channels;
#pragma warning disable 0414 //private field assigned but not used.
    IntPtr encoding;
    IntPtr id3v1;
    IntPtr id3v2;
    IntPtr done;
#pragma warning restore 0414 //private field assigned but not used.

    //Consts: Standard values used in almost all conversions.
    private const float const_1_div_128_ = 1.0f / 128.0f;  // 8 bit multiplier
    private const float const_1_div_32768_ = 1.0f / 32768.0f; // 16 bit multiplier
    private const double const_1_div_2147483648_ = 1.0 / 2147483648.0; // 32 bit

    public AudioClip MP3Import(string mPath)
    {
        Debug.LogWarning(mPath);
        MPGImport.mpg123_init();
        IntPtr handle_mpg = MPGImport.mpg123_new(null, new IntPtr());
        MPGImport.mpg123_open(handle_mpg, mPath);
        MPGImport.mpg123_getformat(handle_mpg, out rate, out channels, out encoding);
        int intRate = rate.ToInt32();
        int intChannels = channels.ToInt32();
        //int intEncoding = encoding.ToInt32();

        MPGImport.mpg123_id3(handle_mpg, out id3v1, out id3v2);
        MPGImport.mpg123_format_none(handle_mpg);
        MPGImport.mpg123_format(handle_mpg, intRate, intChannels, 208);

        int FrameSize = MPGImport.mpg123_outblock(handle_mpg);
        byte[] Buffer = new byte[FrameSize];
        int lengthSamples = MPGImport.mpg123_length(handle_mpg);

        AudioClip myClip = AudioClip.Create("myClip", lengthSamples, intChannels, intRate, false);

        int importIndex = 0;

        while (0 == MPGImport.mpg123_read(handle_mpg, Buffer, FrameSize, out done))
        {


            float[] fArray;
            fArray = ByteToFloat(Buffer);

            myClip.SetData(fArray, (importIndex * fArray.Length) / 2);

            importIndex++;
        }

        MPGImport.mpg123_close(handle_mpg);

        return myClip;
    }

    #region Conversions
    public float[] IntToFloat(Int16[] from)
    {
        float[] to = new float[from.Length];

        for (int i = 0; i < from.Length; i++)
            to[i] = (float)(from[i] * const_1_div_32768_);

        return to;
    }
    public Int16[] ByteToInt16(byte[] buffer)
    {
        Int16[] result = new Int16[1];
        int size = buffer.Length;
        if ((size % 2) != 0)
        {
            /* Error here */
            Console.WriteLine("error");
            return result;
        }
        else
        {
            result = new Int16[size / 2];
            IntPtr ptr_src = Marshal.AllocHGlobal(size);
            Marshal.Copy(buffer, 0, ptr_src, size);
            Marshal.Copy(ptr_src, result, 0, result.Length);
            Marshal.FreeHGlobal(ptr_src);
            return result;
        }
    }
    public float[] ByteToFloat(byte[] bArray)
    {
        Int16[] iArray;

        iArray = ByteToInt16(bArray);

        return IntToFloat(iArray);
    }
    #endregion
    #endregion
}
