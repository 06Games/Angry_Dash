using AngryDash.Language;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoSettings : MonoBehaviour
{
    public Transform GraphicalOptions;

    public static bool mobile()
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }

    void Start() { NewStart(); }
    public void NewStart()
    {
        ShowFPS(ConfigAPI.GetBool("video.showFPS"));

        if (mobile())
        {
            MaxFPS(0);
            GraphicalOptions.GetChild(1).gameObject.SetActive(false);
        }
        else MaxFPS(ConfigAPI.GetInt("video.maxFPSValue"));

        MSAA(ConfigAPI.GetInt("video.MSAALevel"));

        if (mobile())
        {
            FullScreen(true);
            GraphicalOptions.GetChild(3).gameObject.SetActive(false);
        }
        else FullScreen(ConfigAPI.GetBool("video.fullScreen"));

        APNG(ConfigAPI.GetBool("video.APNG"));
    }

    public void ShowFPS(Toggle _Toggle) { ShowFPS(_Toggle.isOn, _Toggle); }
    void ShowFPS(bool show, Toggle _Toggle = null)
    {
        ConfigAPI.SetBool("video.showFPS", show);
        if (_Toggle == null)
            GraphicalOptions.GetChild(0).GetComponent<Toggle>().isOn = show;
    }

    public void MaxFPS(Slider _Slider) { MaxFPS(_Slider.value, _Slider); }
    void MaxFPS(float value, Slider _Slider = null)
    {
        int FPS = (int)value;
        if (_Slider != null)
            FPS = (int)(value * 60F);

        if (value == -1 | value == 7)
            FPS = -1;

        if (FPS == 0)
            QualitySettings.vSyncCount = 1;
        else
        {
            QualitySettings.vSyncCount = 0;
            if (FPS == -1)
                Application.targetFrameRate = 100000000; //Presque sans limite
            else Application.targetFrameRate = FPS;
        }

        if (_Slider == null)
        {
            _Slider = GraphicalOptions.GetChild(1).GetComponent<Slider>();
            if (value >= 0) _Slider.value = value / 60;
            else _Slider.value = 7;
        }
        string Text = LangueAPI.Get("native", "SettingsVideoFpsLimit", "[0] FPS", FPS);
        if (value == 0)
            Text = LangueAPI.Get("native", "SettingsVideoFpsLimitV-Sync", "V-Sync");
        else if (FPS == -1)
            Text = LangueAPI.Get("native", "SettingsVideoFpsLimitUnlimited", "Unlimited");
        _Slider.transform.GetChild(0).GetComponent<Text>().text = Text;

        ConfigAPI.SetInt("video.maxFPSValue", FPS);
    }

    public void MSAA(Dropdown _Dropdown) { MSAA(_Dropdown.value, _Dropdown); }
    void MSAA(int value, Dropdown _Dropdown = null)
    {
        if (_Dropdown == null)
        {
            _Dropdown = GraphicalOptions.GetChild(2).GetComponent<Dropdown>();
            _Dropdown.value = (int)Mathf.Log(value, 2);
            List<Dropdown.OptionData> OD = _Dropdown.options;
            OD[0].text = LangueAPI.Get("native", "SettingsVideoAntiAliasingDisabled", "Disabled");
            for (int i = 1; i < OD.ToArray().Length; i++)
                OD[i].text = LangueAPI.Get("native", "SettingsVideoAntiAliasing", "MSAA [0]x", Mathf.Pow(2, i));

            _Dropdown.options = OD;
        }
        else value = (int)Mathf.Pow(2, value);

        if (value == 1)
            value = 0;
        ConfigAPI.SetInt("video.MSAALevel", value);
    }

    public void FullScreen(Toggle _Toggle) { FullScreen(_Toggle.isOn, _Toggle); }
    public void FullScreen(bool on, Toggle _Toggle = null)
    {
        ConfigAPI.SetBool("video.fullScreen", on);
        if (on) Display.Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        else Display.Screen.SetResolution(1366, 768, false);

        if (_Toggle == null)
            GraphicalOptions.GetChild(3).GetComponent<Toggle>().isOn = on;
    }

    public void APNG(Toggle toggle) { APNG(toggle.isOn, toggle); }
    void APNG(bool on, Toggle toggle = null)
    {
        ConfigAPI.SetBool("video.APNG", on);
        if (toggle == null) GraphicalOptions.GetChild(4).GetComponent<Toggle>().isOn = on;
    }
}
