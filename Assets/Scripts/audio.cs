using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
//using PlayerPrefs = PreviewLabs.PlayerPrefs;

public class audio : MonoBehaviour {

    public AudioMixer mixer;
    public string[] parametersNames = new string[3] { "Master", "Musique", "FX" };

    public int[] Value = new int[3] { -15, -15, -15 };
    public Scrollbar[] scroll;

    void Start()
    {
        for (int i = 0; i < parametersNames.Length; i++)
        {
            Value[i] = PlayerPrefs.GetInt(parametersNames[i]);
            mixer.SetFloat(parametersNames[i], Value[i]);
            scroll[i].value = (Value[i] + 30F) / 30;
        }
    }

    public void ValueChanged(int i)
    {
        if (scroll[i].value == 0)
            Value[i] = -80;
        else Value[i] = (int)(scroll[i].value*30)-30;
        mixer.SetFloat(parametersNames[i], Value[i]);
        PlayerPrefs.SetInt(parametersNames[i], Value[i]);
    }
}
