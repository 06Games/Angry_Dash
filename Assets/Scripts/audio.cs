using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class audio : MonoBehaviour
{
    [Header("Sound level")]
    public AudioMixer mixer;
    public string[] parametersNames = new string[3] { "Master", "Musique", "FX" };
    private string[] parametersConfigNames = { "audio.master", "audio.music", "audio.fx" };

    public int[] Value = new int[3] { -15, -15, -15 };
    public Scrollbar[] scroll;

    [Header("Other parameters")]
    public Toggle FullyLoaded;

    private void Start() { NewStart(); }

    private void NewStart()
    {
        for (var i = 0; i < parametersNames.Length; i++)
        {
            if (string.IsNullOrEmpty(ConfigAPI.GetString(parametersConfigNames[i]))) ConfigAPI.SetInt(parametersConfigNames[i], 0);

            Value[i] = ConfigAPI.GetInt(parametersConfigNames[i]);
            mixer.SetFloat(parametersNames[i], Value[i]);
            scroll[i].value = (Value[i] + 30F) / 30;
        }

        FullyLoaded.isOn = ConfigAPI.GetBool("audio.WaitUntilFullyLoaded");
    }

    public void ValueChanged(int i)
    {
        if (scroll[i].value == 0)
            Value[i] = -80;
        else Value[i] = (int)(scroll[i].value * 30) - 30;
        mixer.SetFloat(parametersNames[i], Value[i]);
        ConfigAPI.SetInt(parametersConfigNames[i], Value[i]);
    }

    public void WaitUntilFullyLoaded(Toggle toggle) { ConfigAPI.SetBool("audio.WaitUntilFullyLoaded", toggle.isOn); }
}
