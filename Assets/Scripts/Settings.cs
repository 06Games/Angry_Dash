using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public Transform GraphicalOptions;

    public static bool mobile() {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        return true;
#else 
        return false;
#endif
    }

    private void Start()
    {
        ShowFPS(ConfigAPI.GetBool("FPS.show"));

        if (mobile())
        {
            MaxFPS(0);
            GraphicalOptions.GetChild(1).gameObject.SetActive(false);
        }
        else MaxFPS(ConfigAPI.GetInt("FPS.maxValue"));

        MSAA(ConfigAPI.GetInt("MSAA.level"));

        if (mobile())
        {
            FullScreen(true);
            GraphicalOptions.GetChild(3).gameObject.SetActive(false);
        }
        else FullScreen(ConfigAPI.GetBool("window.fullscreen"));
    }

    public void ShowFPS(Toggle _Toggle) { ShowFPS(_Toggle.isOn, _Toggle); }
    void ShowFPS(bool show, Toggle _Toggle = null)
    {
        ConfigAPI.SetBool("FPS.show", show);
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
        string Text = LangueAPI.StringWithArgument("graphicalMaximumFps", FPS);
        if (value == 0)
            Text = LangueAPI.String("graphicalMaximumFpsV-Sync");
        else if (FPS == -1)
            Text = LangueAPI.String("graphicalMaximumFpsUnlimited");
        _Slider.transform.GetChild(0).GetComponent<Text>().text = Text;

        ConfigAPI.SetInt("FPS.maxValue", FPS);
    }

    public void MSAA(Dropdown _Dropdown) { MSAA(_Dropdown.value, _Dropdown); }
    void MSAA(int value, Dropdown _Dropdown = null)
    {
        if (_Dropdown == null)
        {
            _Dropdown = GraphicalOptions.GetChild(2).GetComponent<Dropdown>();
            _Dropdown.value = (int)Mathf.Log(value, 2);
            List<Dropdown.OptionData> OD = _Dropdown.options;
            OD[0].text = LangueAPI.String("graphicalAntiAliasingDisabled");
            for (int i = 1; i < OD.ToArray().Length; i++)
                OD[i].text = LangueAPI.StringWithArgument("graphicalAntiAliasing", Mathf.Pow(2, i) + "x");

            _Dropdown.options = OD;
        }
        else value = (int)Mathf.Pow(2, value);

        if (value == 1)
            value = 0;
        ConfigAPI.SetInt("MSAA.level", value);
    }

    public void FullScreen(Toggle _Toggle) { FullScreen(_Toggle.isOn, _Toggle); }
    void FullScreen(bool on, Toggle _Toggle = null)
    {
        ConfigAPI.SetBool("window.fullscreen", on);
        if (on) Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
        else Screen.SetResolution(1366, 768, false);

        if (_Toggle == null)
            GraphicalOptions.GetChild(3).GetComponent<Toggle>().isOn = on;
    }
}
