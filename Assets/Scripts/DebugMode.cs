using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugMode : MonoBehaviour
{
    public Tayx.Graphy.GraphyManager graphy;

    void Start()
    {
        if (ConfigAPI.GetBool("debug.enable"))
        {
            DontDestroyOnLoad(gameObject);
            OpenClose(false);
        }
        else Destroy(gameObject);
    }

    public void OpenClose(bool open)
    {
        LoadingScreenControl.CanChange = !open;
        if (open)
        {
            Transform content = transform.GetChild(1).GetChild(2).GetComponent<ScrollRect>().content;

            DeviceInfo(content.GetChild(content.childCount - 1).GetChild(1).GetComponent<Text>());
        }

        transform.GetChild(0).gameObject.SetActive(!open);
        transform.GetChild(1).gameObject.SetActive(open);
    }

    public void Graphy(Toggle toggle)
    {
        if (toggle.isOn) graphy.Enable();
        else graphy.Disable();
    }

    public void ShowLogs(Toggle toggle)
    {
        Transform content = transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<ScrollRect>().content;
        GameObject template = content.GetChild(0).gameObject;
        void Logs (object sender, Tools.BetterEventArgs e)
        {
            string type = "";
            LogType logType = (LogType)sender;
            if (logType == LogType.Log) type = "<color=grey>Info: </color>";
            else if (logType == LogType.Warning) type = "<color=orange>Warning: </color>";
            else if (logType == LogType.Error | logType == LogType.Exception) type = "<color=red>Error: </color>";
            else if (logType == LogType.Assert) type = "<color=white>Unkown: </color>";


            if (content.childCount >= 5) Destroy(content.GetChild(1).gameObject);
            GameObject go = Instantiate(template, content);
            go.transform.GetChild(0).GetComponent<Text>().text = type + (string)e.UserState;
            go.SetActive(true);
            StartCoroutine(LogAutoSuppr(go));
        }
        
        if (toggle.isOn) Logging.NewMessage += Logs;
        else Logging.NewMessage = null;
    }
    IEnumerator LogAutoSuppr(GameObject go)
    {
        yield return new WaitForSeconds(5);
        if (go != null)
        {
            Image text = go.GetComponent<Image>();
            while (text.color.a > 0)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - 0.05F);
                yield return new WaitForEndOfFrame();
            }
            Destroy(go);
        }
    }

    bool Coordinates;
    public void ShowCoordinates(Toggle toggle)
    {
        if (toggle.isOn)
        {
            StartCoroutine(CoordinatesRefresh(toggle, transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>()));
            LoadingScreenControl.OnSceneChange += (sender, e) => Coordinates = (string)e.UserState == "Player";
        }
    }
    IEnumerator CoordinatesRefresh(Toggle toggle, Text text, GameObject player = null)
    {
        if (player == null & Coordinates) player = GameObject.Find("Main Camera").GetComponent<MainCam>().Player;
        if(Coordinates) text.gameObject.SetActive(true);

        Vector2 playerPos = new Vector2(25, 25);
        if (player != null) playerPos = player.transform.position;
        for (int i = 0; i < 2; i++) playerPos[i] = (playerPos[i] - 25) / 50F;
        text.text = playerPos.ToString("0.0");

        if (!Coordinates)
        {
            text.gameObject.SetActive(false);
            yield return new WaitUntil(() => Coordinates);
        }
        else yield return new WaitForEndOfFrame();

        if (toggle.isOn) StartCoroutine(CoordinatesRefresh(toggle, text, player));
        else text.gameObject.SetActive(false);
    }

    public void DeviceInfo(Text text)
    {
        Resolution res = Screen.currentResolution;
        text.text = "Screen: " + res.width + "x" + res.height + "@" + res.refreshRate + "Hz"
            + "\nGraphics API: " + SystemInfo.graphicsDeviceVersion
            + "\nGPU: " + SystemInfo.graphicsDeviceName
            + "\nVRAM: " + SystemInfo.graphicsMemorySize + "MB. Max texture size: " + SystemInfo.maxTextureSize + "px. Shader level: " + SystemInfo.graphicsShaderLevel
            + "\nCPU: " + SystemInfo.processorType + " [" + SystemInfo.processorCount + " cores]"
            + "\nRAM: " + SystemInfo.systemMemorySize + " MB"
            + "\nOS: " + SystemInfo.operatingSystem + " [" + SystemInfo.deviceType + "]";
    }
}
