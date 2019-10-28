using AngryDash.Language;
using FileFormat.XML;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using Tools;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DependenciesManager : MonoBehaviour
{
    #region RPs
    void Start()
    {
        LoadingScreenControl LSC = FindObjectOfType<LoadingScreenControl>();
        string[] args = LSC.GetArgs();
        if (args == null) return;
        if (args.Length >= 2)
        {
            if (args[0] == "Dependencies")
            {
                //Disable start-up actions
                for (int i = 0; i < transform.parent.childCount; i++)
                {
                    GameObject child = transform.parent.GetChild(i).gameObject;
                    if (child != gameObject) child.SetActive(false);
                }
                StartCoroutine(DownloadRPs(() => LSC.LoadScreen(args[1]), new Item[] { new Item(new XML(args[2]).RootElement.node) }));
            }
        }
    }

    public void DownloadRPs(Action complete) { StartCoroutine(DownloadRPs(complete, null)); }
    public System.Collections.IEnumerator DownloadRPs(Action complete, Item[] downloadList)
    {
        if (InternetAPI.IsConnected())
        {
            string ressourcesURL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/ressources/";
            string ressourcesPath = Application.persistentDataPath + "/Ressources/";
            if (!Directory.Exists(ressourcesPath)) Directory.CreateDirectory(ressourcesPath);

            var DownloadPanel = transform.GetChild(1).gameObject;
            var slider = DownloadPanel.transform.GetChild(1).GetComponent<Slider>();
            slider.transform.GetChild(3).GetComponent<Text>().text = LangueAPI.Get("native", "download.ressourcesPack.connection", "Connection to the server");
            slider.transform.GetChild(4).gameObject.SetActive(false);
            DownloadPanel.SetActive(true);
            Text DownloadInfo = slider.transform.GetChild(4).GetComponent<Text>();

            if (downloadList == null)
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(ressourcesURL + "?v=" + Application.version))
                {
                    yield return webRequest.SendWebRequest();
                    downloadList = new XML(webRequest.downloadHandler.text).RootElement.GetItemsByAttribute("ressource", "required", "TRUE");
                }
            }

            for (int i = 0; i < downloadList.Length; i++)
            {
                slider.value = i / downloadList.Length;
                slider.transform.GetChild(3).GetComponent<Text>().text = LangueAPI.Get("native", "download.ressourcesPack.state", "File: [0] / [1]", i + 1, downloadList.Length);

                string rpName = Path.GetFileNameWithoutExtension(downloadList[i].GetItem("name").Value);
                var rpDir = new DirectoryInfo(ressourcesPath + rpName + "/");
                if (rpDir.Exists && rpDir.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length) == downloadList[i].GetItem("size").value<long>()) continue;

                string rpURL = downloadList[i].GetItem("name").Value;
                if (!Uri.IsWellFormedUriString(rpURL, UriKind.Absolute)) rpURL = ressourcesURL + rpURL; //If the URL is relative, create an absolute one
                using (UnityWebRequest webRequest = UnityWebRequest.Get(rpURL))
                {
                    var sw = new System.Diagnostics.Stopwatch();
                    webRequest.SendWebRequest();
                    sw.Start();
                    while (!webRequest.isDone)
                    {
                        var unit = new string[] { "B", "KB", "MB", "GB", "TB" };
                        double downloadedSize = webRequest.downloadedBytes;

                        // Download speed
                        double speed = downloadedSize / sw.Elapsed.TotalSeconds;
                        int speedPower = GetCorrectUnit(speed);
                        speed = Math.Round(speed / Mathf.Pow(1000, speedPower), 1);

                        float pourcentage = webRequest.downloadProgress * 100F; //Progress

                        //Downloaded size
                        int sizePower = 0;
                        if (double.TryParse(webRequest.GetResponseHeader("Content-Length"), out double totalSize)) sizePower = GetCorrectUnit(totalSize);
                        else sizePower = GetCorrectUnit(downloadedSize);
                        totalSize = Math.Round(totalSize / Mathf.Pow(1000, sizePower), 1);
                        downloadedSize = Math.Round(downloadedSize / Mathf.Pow(1000, sizePower), 1);


                        //Text
                        string speedText = LangueAPI.Get("native", $"download.speed.{unit[speedPower]}", $"[0] {unit[speedPower]}/s", speed);
                        string downloaded = LangueAPI.Get("native", $"download.state.{unit[sizePower]}", $"[0] {unit[sizePower]} out of [1] {unit[sizePower]}", downloadedSize.ToString(), totalSize > 0 ? totalSize.ToString() : "~");
                        string pourcent = LangueAPI.Get("native", "download.state.percentage", "[0]%", pourcentage.ToString("00"));
                        DownloadInfo.text = speedText + " - " + downloaded + " - <color=grey>" + pourcent + "</color>";
                        DownloadInfo.gameObject.SetActive(true);

                        //Progress bar
                        float baseValue = i / downloadList.Length;
                        float oneValue = 1F / downloadList.Length;
                        slider.value = baseValue + (pourcentage / 100F * oneValue);

                        yield return new WaitForEndOfFrame();
                    }

                    if (string.IsNullOrEmpty(webRequest.error))
                    {
                        string zipPath = Application.temporaryCachePath + "/" + i + ".zip";
                        File.WriteAllBytes(zipPath, webRequest.downloadHandler.data);
                        if (rpDir.Exists) rpDir.Delete(true);
                        FileFormat.ZIP.DecompressAsync(zipPath, ressourcesPath + rpName + "/", () => File.Delete(zipPath)); //Unzip in background and delete the file when it's finished
                    }
                    else Debug.LogError(webRequest.error);
                }
            }

            complete();
        }
        else if (!Directory.Exists(Application.persistentDataPath + "/Ressources/default/")) transform.GetChild(0).gameObject.SetActive(true); //This is the first start, the game can't start
        else complete.Invoke(); //Continue without downloading anything
    }
    public static int GetCorrectUnit(double d) {
        int NbEntier = Math.Round(d, 0).ToString().Length;
        return (int)((NbEntier - 1) / 3F);
    }
    #endregion

    #region Levels
    public void DownloadLevels(Action complete) { StartCoroutine(DownloadLevels(complete, null)); }
    public System.Collections.IEnumerator DownloadLevels(Action complete, Item[] downloadList)
    {
        if (InternetAPI.IsConnected())
        {
            string levelsURL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/solo/";
            string levelsPath = Application.persistentDataPath + "/Levels/Official Levels/";
            if (!Directory.Exists(levelsPath)) Directory.CreateDirectory(levelsPath);

            GameObject DownloadInfo = transform.GetChild(2).gameObject;
            DownloadInfo.SetActive(true);
            var slider = DownloadInfo.transform.GetChild(0).GetComponent<Slider>();
            var text = DownloadInfo.transform.GetChild(2).GetComponent<Text>();

            if (downloadList == null)
            {
                using (UnityWebRequest webRequest = UnityWebRequest.Get(levelsURL + "?v=" + Application.version))
                {
                    yield return webRequest.SendWebRequest();
                    downloadList = new XML(webRequest.downloadHandler.text).RootElement.GetItems("level");
                }
            }

            for (int i = 0; i < downloadList.Length; i++)
            {
                slider.value = (float)i / downloadList.Length;
                text.text = LangueAPI.Get("native", "download.levels.state", "Level: [0] / [1]", i + 1, downloadList.Length);

                string levelName = Path.GetFileName(downloadList[i].GetItem("name").Value);
                string levelURL = downloadList[i].GetItem("name").Value;
                if (!Uri.IsWellFormedUriString(levelURL, UriKind.Absolute)) levelURL = levelsURL + levelURL; //If the URL is relative, create an absolute one
                using (UnityWebRequest webRequest = UnityWebRequest.Get(levelURL))
                {
                    webRequest.SendWebRequest();
                    while (!webRequest.isDone)
                    {
                        //Progress
                        float baseValue = (float)i / downloadList.Length;
                        float oneValue = 1F / downloadList.Length;
                        float total = baseValue + (webRequest.downloadProgress * oneValue);

                        slider.fillRect.GetComponentInChildren<Text>().text = LangueAPI.Get("native", "download.state.percentage", "[0]%", (total * 100F).ToString("00")); //Progress text
                        slider.value = total; //Progress bar

                        yield return new WaitForEndOfFrame();
                    }

                    if (string.IsNullOrEmpty(webRequest.error)) File.WriteAllBytes(levelsPath + levelName, webRequest.downloadHandler.data);
                    else Debug.LogError(webRequest.error);
                }
            }

            complete();
        }
        else if (!Directory.Exists(Application.persistentDataPath + "/Levels/Official Levels/")) transform.GetChild(0).gameObject.SetActive(true); //This is the first start, the game can't start
        else complete.Invoke(); //Continue without downloading anything
    }
    #endregion
}
