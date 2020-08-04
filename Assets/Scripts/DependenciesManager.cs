using AngryDash.Language;
using FileFormat.XML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using RP = RessourcePackManager.RP;

public class DependenciesManager : MonoBehaviour
{
    void Start()
    {
        string[] args = SceneManager.args;
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
                StartCoroutine(DownloadRPs(() => SceneManager.LoadScene(args[1]), new[] { Utils.XMLtoClass<RP>(args[2]) }));
            }
        }
    }

    public void DownloadRPs(Action complete) { StartCoroutine(DownloadRPs(complete, null)); }
    public IEnumerator DownloadRPs(Action complete, RP[] downloadList)
    {
        string ressourcesPath = Application.persistentDataPath + "/Ressources/";
        if (!Directory.Exists(ressourcesPath)) Directory.CreateDirectory(ressourcesPath);

        bool firstStart = !Directory.Exists(ressourcesPath + "default/");
        if (!firstStart && downloadList == null) { complete?.Invoke(); yield break; } //This isn't the first start and the user hasn't requested to download a RP
        if (firstStart && !InternetAPI.IsConnected()) { transform.GetChild(0).gameObject.SetActive(true); yield break; } //This is the first start, the game can't start

        //Initialization
        var DownloadPanel = transform.GetChild(1).gameObject;
        var slider = DownloadPanel.transform.GetChild(1).GetComponent<Slider>();
        slider.transform.GetChild(3).GetComponent<Text>().text = LangueAPI.Get("native", "download.ressourcesPack.connection", "Connection to the server");
        slider.transform.GetChild(4).gameObject.SetActive(false);
        DownloadPanel.SetActive(true);
        Text DownloadInfo = slider.transform.GetChild(4).GetComponent<Text>();

        if (downloadList == null) //First start, we get the list of required RPs
            yield return _06Games.ServerAPI.ClassCoroutine<Dictionary<string, RP>>($"angry-dash/ressources/?gameVersion={Application.version}", (r) => downloadList = r.Values.ToArray());

        yield return Download(DownloadInfo, slider, downloadList, !firstStart, (rp, handle) =>
        {
            string zipPath = Application.temporaryCachePath + "/" + rp.folderName + ".zip";
            File.WriteAllBytes(zipPath, handle.data);
            var rpDir = new DirectoryInfo(ressourcesPath + rp.folderName);
            if (rpDir.Exists) rpDir.Delete(true);
            FileFormat.ZIP.DecompressAsync(zipPath, ressourcesPath + rp.folderName + "/", () => File.Delete(zipPath)); //Unzip in background and delete the file when it's finished
        }, (rp) =>
        {
            var rpDir = new DirectoryInfo(ressourcesPath + rp.folderName);
            return rpDir.Exists && rpDir.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length) == rp.size;
        });

        DownloadPanel.SetActive(false);
        complete();
    }

    public IEnumerator DownloadLevels(Action complete)
    {
        if (!InternetAPI.IsConnected())
        {
            if (Directory.Exists(Application.persistentDataPath + "/Levels/Official Levels/")) complete.Invoke(); //This isn't the first start so the game can start
            else transform.GetChild(0).gameObject.SetActive(true); //This is the first start, the game can't start
            yield break;
        }

        string levelsPath = Application.persistentDataPath + "/Levels/Official Levels/";
        if (!Directory.Exists(levelsPath)) Directory.CreateDirectory(levelsPath);

        GameObject DownloadPanel = transform.GetChild(2).gameObject;
        DownloadPanel.SetActive(true);
        var slider = DownloadPanel.transform.GetChild(0).GetComponent<Slider>();
        slider.transform.GetChild(3).GetComponent<Text>().text = LangueAPI.Get("native", "download.ressourcesPack.connection", "Connection to the server");
        slider.value = 0;

        RP[] downloadList = null;
        yield return _06Games.ServerAPI.ClassCoroutine<Dictionary<string, RP>>($"angry-dash/levels/official/?gameVersion={Application.version}", (r) => downloadList = r.Select(pair =>
        {
            var rp = pair.Value;
            rp.name = pair.Key;
            rp.folderName = $"{pair.Key}.level";
            return rp;
        }).ToArray());

        yield return Download(null, slider, downloadList, true, (rp, handle) => File.WriteAllBytes(levelsPath + rp.folderName, handle.data), (rp) =>
        {
            var levelFile = new FileInfo(levelsPath + rp.folderName);
            return levelFile.Exists && Security.Hashing.SHA(Security.Hashing.Algorithm.SHA256, levelFile.OpenRead()) == rp.sha256;
        });


        DownloadPanel.SetActive(false);
        complete();
    }


    IEnumerator Download(Text DownloadInfo, Slider slider, RP[] downloadList, bool canAbort, Action<RP, DownloadHandler> downloadComplete, Func<RP, bool> skip = null)
    {
        for (int i = 0; i < downloadList.Length; i++)
        {
            var rp = downloadList[i];
            slider.value = i / downloadList.Length;
            slider.transform.GetChild(3).GetComponent<Text>().text = LangueAPI.Get("native", "download.ressourcesPack.state", "File: [0] / [1]", i + 1, downloadList.Length);

            if (skip != null && skip(rp)) continue;

            var downloader = UnityWebRequest.Get(rp.url);
            var sw = new System.Diagnostics.Stopwatch();
            downloader.SendWebRequest();
            sw.Start();

            string speedText = "";
            var lastSpeedUpdate = TimeSpan.Zero;
            while (!downloader.isDone)
            {
                double downloadedS = downloader.downloadedBytes;
                float pourcentage = downloader.downloadProgress * 100F; //Progress

                //Downloaded size
                int sizePower = double.TryParse(downloader.GetResponseHeader("Content-Length"), out double totalS) ? GetCorrectUnit(totalS) : GetCorrectUnit(downloadedS);
                var totalSize = FileSizeUnit(totalS, sizePower);
                var downloadedSize = FileSizeUnit(downloadedS, sizePower);

                //Text
                if ((sw.Elapsed - lastSpeedUpdate).TotalSeconds > 1) //Update speed each second
                {
                    var speed = FileSizeUnit(downloadedS / sw.Elapsed.TotalSeconds);
                    speedText = LangueAPI.Get("native", "download.speed", "[0]/s", speed);
                    lastSpeedUpdate = sw.Elapsed;
                }
                string downloaded = LangueAPI.Get("native", "download.state.unit", $"[0] out of [1]", downloadedSize, totalS > 0 ? totalSize : "~");
                string pourcent = LangueAPI.Get("native", "download.state.percentage", "[0]%", pourcentage.ToString("00"));
                if (DownloadInfo != null)
                {
                    DownloadInfo.text = speedText + " - " + downloaded + " - <color=grey>" + pourcent + "</color>";
                    DownloadInfo.gameObject.SetActive(true);
                }

                //Progress bar
                float baseValue = i / downloadList.Length;
                float oneValue = 1F / downloadList.Length;
                slider.value = baseValue + (pourcentage / 100F * oneValue);

                if (canAbort && Input.GetKey(KeyCode.Escape)) //If the user wants to skip and the default RP already exists
                {
                    downloader.Abort(); //Stop the download
                    yield break; //Don't download any other RPs
                }
                yield return new WaitForEndOfFrame();
            }

            if (string.IsNullOrEmpty(downloader.error)) downloadComplete?.Invoke(rp, downloader.downloadHandler);
            else if (downloader.error != "Request aborted") Debug.LogError(downloader.error);
        }
    }

    public static string FileSizeUnit(double size, int? unitPower = null)
    {
        if (unitPower == null) unitPower = GetCorrectUnit(size);
        var value = Math.Round(size / Mathf.Pow(1000, unitPower.Value), 1);
        var unit = new string[] { "B", "KB", "MB", "GB", "TB" }[unitPower.Value];
        return LangueAPI.Get("native", $"download.unit.{unit}", $"[0] {unit}", value.ToString("0.#"));
    }
    public static int GetCorrectUnit(double d)
    {
        int NbEntier = Math.Round(d, 0).ToString().Length;
        return (int)((NbEntier - 1) / 3F);
    }
}
