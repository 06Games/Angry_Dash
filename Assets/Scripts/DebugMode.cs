﻿using System.Collections;
using AngryDash.Game;
using Tayx.Graphy;
using UnityEngine;
using UnityEngine.UI;

public class DebugMode : MonoBehaviour
{
    public GraphyManager graphy;

    private void Start()
    {
        if (ConfigAPI.GetBool("debug.enable") & FindObjectsOfType<DebugMode>().Length <= 1)
        {
            DontDestroyOnLoad(gameObject);
            Actualize();
            OpenClose(false);
        }
        else Destroy(gameObject);
    }

    public void OpenClose(bool open)
    {
        SceneManager.CanChange = !open;
        if (open)
        {
            Transform content = transform.GetChild(1).GetChild(2).GetComponent<ScrollRect>().content;
            DeviceInfo(content.GetChild(content.childCount - 1).GetChild(1).GetComponent<Text>());
        }

        transform.GetChild(0).gameObject.SetActive(!open);
        transform.GetChild(1).gameObject.SetActive(open);
    }

    public void Actualize() { Actualize(transform.GetChild(1).GetChild(2).GetComponent<ScrollRect>().content); }

    private void Actualize(Transform content)
    {
        UnityThread.executeInUpdate(() =>
        {
            content.GetChild(0).GetComponent<Toggle>().isOn = ConfigAPI.GetBool("debug.graphy");
            graphy.Enable();
            Graphy(ConfigAPI.GetBool("debug.graphy"));
        });
        content.GetChild(1).GetComponent<Toggle>().isOn = ConfigAPI.GetBool("debug.showLogs");
        ShowLogs(ConfigAPI.GetBool("debug.showLogs"));
        content.GetChild(2).GetComponent<Toggle>().isOn = ConfigAPI.GetBool("debug.showCoordinates");
        ShowCoordinates(ConfigAPI.GetBool("debug.showCoordinates"));
    }

    public void Graphy(Toggle toggle) { Graphy(toggle.isOn); }

    private void Graphy(bool on)
    {
        if (on) graphy.Enable();
        else graphy.Disable();

        ConfigAPI.SetBool("debug.graphy", on);
    }

    public void ShowLogs(Toggle toggle) { ShowLogs(toggle.isOn); }

    private void ShowLogs(bool on)
    {
        Transform content = transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<ScrollRect>().content;
        var template = content.GetChild(0).gameObject;
        void Logs(LogType logType, string scene)
        {
            var type = "";
            if (logType == LogType.Log) type = "<color=grey>Info: </color>";
            else if (logType == LogType.Warning) type = "<color=orange>Warning: </color>";
            else if (logType == LogType.Error | logType == LogType.Exception) type = "<color=red>Error: </color>";
            else if (logType == LogType.Assert) type = "<color=white>Unkown: </color>";


            if (content.childCount >= 5) Destroy(content.GetChild(1).gameObject);
            var go = Instantiate(template, content);
            go.transform.GetChild(0).GetComponent<Text>().text = type + scene;
            go.SetActive(true);
            StartCoroutine(LogAutoSuppr(go));
        }

        Logging.NewMessage = null;
        if (on) Logging.NewMessage += Logs;
        else Logging.NewMessage = null;

        ConfigAPI.SetBool("debug.showLogs", on);
    }

    private IEnumerator LogAutoSuppr(GameObject go)
    {
        yield return new WaitForSeconds(5);
        if (go != null)
        {
            var text = go.GetComponent<Image>();
            while (text != null && text.color.a > 0)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - 0.05F);
                yield return new WaitForEndOfFrame();
            }
            Destroy(go);
        }
    }

    private bool Coordinates;
    public void ShowCoordinates(Toggle toggle) { ShowCoordinates(toggle.isOn); }

    private void ShowCoordinates(bool on)
    {
        ConfigAPI.SetBool("debug.showCoordinates", on);

        if (on)
        {
            StartCoroutine(CoordinatesRefresh(transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>()));
            SceneManager.OnSceneChange += scene => Coordinates = scene.name == "Player";
        }
    }

    private IEnumerator CoordinatesRefresh(Text text, GameObject player = null)
    {
        var on = ConfigAPI.GetBool("debug.showCoordinates");

        if (player == null & Coordinates) player = Player.userPlayer.gameObject;
        if (Coordinates) text.gameObject.SetActive(true);

        var playerPos = new Vector2(25, 25);
        if (player != null) playerPos = player.transform.position;
        for (var i = 0; i < 2; i++) playerPos[i] = (playerPos[i] - 25) / 50F;
        text.text = playerPos.ToString("0.0");

        if (!Coordinates)
        {
            text.gameObject.SetActive(false);
            yield return new WaitUntil(() => Coordinates);
        }
        else yield return new WaitForEndOfFrame();

        if (on) StartCoroutine(CoordinatesRefresh(text, player));
        else text.gameObject.SetActive(false);
    }

    public void DeviceInfo(Text text)
    {
        var res = Screen.currentResolution;
        text.text = "Screen: " + res.width + "x" + res.height + "@" + res.refreshRate + "Hz. DPI: " + Screen.dpi
            + "\nGraphics API: " + SystemInfo.graphicsDeviceVersion
            + "\nGPU: " + SystemInfo.graphicsDeviceName
            + "\nVRAM: " + SystemInfo.graphicsMemorySize + "MB. Max texture size: " + SystemInfo.maxTextureSize + "px. Shader level: " + SystemInfo.graphicsShaderLevel
            + "\nCPU: " + SystemInfo.processorType + " [" + SystemInfo.processorCount + " cores]"
            + "\nRAM: " + SystemInfo.systemMemorySize + " MB"
            + "\nOS: " + SystemInfo.operatingSystem + " [" + SystemInfo.deviceType + "]";
    }
}
