using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class DependenciesManager : MonoBehaviour
{

    GameObject DownloadPanel;
    Slider slider;

    public Social _Social;

    void Start() { }

    public void DownloadDefaultsRP()
    {
        DownloadPanel = transform.GetChild(1).gameObject;
        slider = DownloadPanel.transform.GetChild(1).GetComponent<Slider>();

        if (InternetAPI.IsConnected())
        {
            string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/ressources/";
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string Result = "";
            try { Result = client.DownloadString(URL).Replace("<BR />", "\n"); } //Try to access to the server
            catch (Exception e)
            { //else
                UnityEngine.Debug.LogError(e.Message); //log error
                wc_DownloadFileCompleted("pass", new AsyncCompletedEventArgs(null, false, null)); //continue game starting
                return; //stop this function
            }
            string[] lines = Result.Split(new string[1] { "\n" }, StringSplitOptions.None);

            DownloadPanel.SetActive(true);
            slider.transform.GetChild(4).gameObject.SetActive(false);

            downloadFile(0, lines, URL, Application.persistentDataPath + "/Ressources/");
        }
        else wc_DownloadFileCompleted("pass", new AsyncCompletedEventArgs(null, false, null));
    }

    string[] downData = new string[4] { "", "", "", "" };
    Stopwatch sw = new Stopwatch();
    public void downloadFile(int actual, string[] lines, string url, string mainPath)
    {
        string version = lines[actual].Split(new string[] { "</version>" }, StringSplitOptions.None)[0].Replace("<version>", "");
        string name = lines[actual].Split(new string[] { "<name>" }, StringSplitOptions.None)[1].Split(new string[] { "</name>" }, StringSplitOptions.None)[0];
        int size = int.Parse(lines[actual].Split(new string[] { "<size>" }, StringSplitOptions.None)[1].Split(new string[] { "B</size>" }, StringSplitOptions.None)[0]);

        UnityThread.executeInUpdate(() =>
        {
            slider.value = (actual + 1) / lines.Length;
            slider.transform.GetChild(3).GetComponent<Text>().text = LangueAPI.StringWithArgument("native", "downloadRPNumber", new string[2] { (actual + 1).ToString(), lines.Length.ToString() }, "File : [0] / [1]");
        });


        bool down = false;
        if (!CheckVersionCompatibility(version)) down = false;
        else if (!Directory.Exists(Application.persistentDataPath + "/Ressources/default/")) down = true;
        else
        {
            long dirSize = new DirectoryInfo(Application.persistentDataPath + "/Ressources/default/").GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
            if (dirSize == size) down = false;
            else
            {
                down = true;
                Directory.Delete((Application.persistentDataPath + "/Ressources/default/"), true);
            }
        }
        if (down)
        {
            if (!Directory.Exists(mainPath)) Directory.CreateDirectory(mainPath);

            using (WebClient wc = new WebClient())
            {
                sw.Start();
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new Uri(url + name), Application.temporaryCachePath + "/" + actual + ".zip");
            }
        }
        else
        {
            slider.transform.GetChild(4).gameObject.SetActive(false);
            wc_DownloadFileCompleted(null, new AsyncCompletedEventArgs(null, false, null));
        }

        string newS = "";
        for (int i = 0; i < lines.Length; i++)
        {
            if (i < lines.Length - 1)
                newS = newS + lines[i] + "\n";
            else newS = newS + lines[i];
        }

        downData[0] = actual.ToString();
        downData[1] = newS;
        downData[2] = mainPath;
        downData[3] = url;
    }

    private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        // Vitesse
        double speed = Math.Round((e.BytesReceived / sw.Elapsed.TotalSeconds), 1);
        int SpeedReduct = 0;
        for (int i = 0; NbChiffreEntier(speed) > 3 & i <= 4; i++)
        {
            speed = speed / 1024d;
            SpeedReduct++;
        }
        speed = Math.Round(speed, 1);
        string[] SpeedLangueID = new string[] { "downloadSpeedB", "[0] B/s" };
        if (SpeedReduct == 0) SpeedLangueID = new string[] { "downloadSpeedB", "[0] B/s" };
        else if (SpeedReduct == 1) SpeedLangueID = new string[] { "downloadSpeedKB", "[0] KB/s" };
        else if (SpeedReduct == 2) SpeedLangueID = new string[] { "downloadSpeedMB", "[0] MB/s" };
        else if (SpeedReduct == 3) SpeedLangueID = new string[] { "downloadSpeedGB", "[0] GB/s" };
        else if (SpeedReduct == 4) SpeedLangueID = new string[] { "downloadSpeedTB", "[0] TB/s" };


        int pourcentage = e.ProgressPercentage; //Progression

        //Avancé (x Mo sur x Mo)
        double Actual = e.BytesReceived;
        double Total = e.TotalBytesToReceive;
        int Reduct = 0;
        for (int i = 0; NbChiffreEntier(Total) > 3 & i <= 4; i++)
        {
            Actual = Actual / 1024d;
            Total = Total / 1024d;
            Reduct++;
        }
        Actual = Math.Round(Actual, 1);
        Total = Math.Round(Total, 1);
        string[] LangueID = new string[] { "downloadStateB", "[0] B out of [1] B" };
        if (Reduct == 0) LangueID = new string[] { "downloadStateB", "[0] B out of [1] B" };
        else if (Reduct == 1) LangueID = new string[] { "downloadStateKB", "[0] KB out of [1] KB" };
        else if (Reduct == 2) LangueID = new string[] { "downloadStateMB", "[0] MB out of [1] MB" };
        else if (Reduct == 3) LangueID = new string[] { "downloadStateGB", "[0] GB out of [1] GB" };
        else if (Reduct == 4) LangueID = new string[] { "downloadStateTB", "[0] TB out of [1] TB" };

        UnityThread.executeInUpdate(() =>
        {
            string speedText = LangueAPI.StringWithArgument("native", SpeedLangueID[0], speed, SpeedLangueID[1]);
            string downloaded = LangueAPI.StringWithArgument("native", LangueID[0], new string[] { Actual.ToString("0.0"), Total.ToString("0.0") }, LangueID[1]);
            string pourcent = LangueAPI.StringWithArgument("native", "downloadStatePercentage", pourcentage.ToString("00"), "[0]%");

            Text DownloadInfo = slider.transform.GetChild(4).GetComponent<Text>();
            DownloadInfo.gameObject.SetActive(true);
            DownloadInfo.text = speedText + " - " + downloaded + " - <color=grey>" + pourcent + "</color>";

            //Progression plus détaillée
            string[] lines = downData[1].Split(new string[1] { "\n" }, StringSplitOptions.None);
            float baseValue = (int.Parse(downData[0]) + 1) / lines.Length;
            float oneValue = 1 / (float)lines.Length;
            slider.value = baseValue + ((pourcentage / 100F) * oneValue);
        });
    }

    private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {
        sw.Reset();
        if (e.Cancelled)
        {
            print("The download has been cancelled");
            return;
        }

        if (e.Error != null) // We have an error ! Retry a few times, then abort.
        {
            print("An error ocurred while trying to download file\n" + e.Error);

            return;
        }

        UnityThread.executeInUpdate(() =>
        {
            string[] s = downData[1].Split(new string[1] { "\n" }, StringSplitOptions.None);

            bool c = false;
            if (sender == null) c = true;
            else if (sender.ToString() != "pass") c = true;
            else
            {
                UnityThread.executeInUpdate(() =>
                {
                    downloadLevels();
                    DownloadPanel.SetActive(false);
                });
            }

            if (c)
            {
                if (sender != null)
                {
                    string[] pathTo = s[int.Parse(downData[0])].Split(new string[] { "<name>" }, StringSplitOptions.None)[1].Split(new string[] { "</name>" }, StringSplitOptions.None)[0].Split(new string[] { "/", "\\" }, StringSplitOptions.None);
                    FileFormat.ZIP.Decompress(Application.temporaryCachePath + "/" + downData[0] + ".zip", downData[2] + pathTo[pathTo.Length - 1].Replace(".zip", "") + "/");
                }

                try
                {
                    if (int.Parse(downData[0]) < s.Length - 2)
                        downloadFile(int.Parse(downData[0]) + 1, s, downData[3], downData[2]);
                    else wc_DownloadFileCompleted("pass", new AsyncCompletedEventArgs(null, false, null));
                }
                catch { wc_DownloadFileCompleted("pass", new AsyncCompletedEventArgs(null, false, null)); }
            }
        });
    }

    #region Levels
    int data_actual;
    string[] data_lines;
    public void downloadLevels(int actual = 0, string[] lines = null)
    {
        if (InternetAPI.IsConnected())
        {
            if (lines == null)
            {
                string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/solo/";
                WebClient client = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                string Result = "";
                try { Result = client.DownloadString(URL).Replace("<BR />", "\n"); } catch { levels_DownloadFileCompleted("pass", new AsyncCompletedEventArgs(null, false, null)); return; }
                lines = Result.Split(new string[1] { "\n" }, StringSplitOptions.None);
            }

            UnityThread.executeInUpdate(() =>
            {
                GameObject go = transform.GetChild(2).gameObject;
                go.SetActive(true);
                go.transform.GetChild(0).GetComponent<Slider>().value = (float)actual / lines.Length;
                go.transform.GetChild(2).GetComponent<Text>().text = actual + "/" + lines.Length;
            });

            string version = lines[actual].Split(new string[] { "</version>" }, StringSplitOptions.None)[0].Replace("<version>", "");
            string name = lines[actual].Split(new string[] { "<name>" }, StringSplitOptions.None)[1].Split(new string[] { "</name>" }, StringSplitOptions.None)[0];
            int size = int.Parse(lines[actual].Split(new string[] { "<size>" }, StringSplitOptions.None)[1].Split(new string[] { "B</size>" }, StringSplitOptions.None)[0]);

            string desktopPath = Application.persistentDataPath + "/Levels/Official Levels/" + name;

            bool down = false;
            if (!CheckVersionCompatibility(version)) down = false;
            else if (File.Exists(desktopPath))
                if (new FileInfo(desktopPath).Length != size) down = true;
                else down = false;
            else down = true;
            if (down)
            {
                if (!Directory.Exists(Application.persistentDataPath + "/Levels/Official Levels/"))
                    Directory.CreateDirectory(Application.persistentDataPath + "/Levels/Official Levels/");

                string url = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/solo/" + name;
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFileCompleted += levels_DownloadFileCompleted;
                    wc.DownloadFileAsync(new Uri(url), desktopPath);
                }
            }
            else levels_DownloadFileCompleted(null, new AsyncCompletedEventArgs(null, false, null));


            data_actual = actual;
            data_lines = lines;
        }
        else levels_DownloadFileCompleted("pass", new AsyncCompletedEventArgs(null, false, null));
    }

    private void levels_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
    {

        if (e.Cancelled)
        {
            print("The download has been cancelled");
            return;
        }

        if (e.Error != null) // We have an error! Retry a few times, then abort.
        {
            print("An error ocurred while trying to download file\n" + e.Error);

            return;
        }

        UnityThread.executeInUpdate(() =>
        {
            bool c = false;
            if (sender == null) c = true;
            else if (sender.ToString() != "pass") c = true;
            else
            {
                    transform.GetChild(2).gameObject.SetActive(false);
                Account ac = GameObject.Find("Account").GetComponent<Account>();
                ac.complete += (s, args) => _Social.NewStart();
                ac.Initialize();
            }

            if (c)
            {
                try
                {
                    if (data_actual < data_lines.Length - 1)
                        downloadLevels(data_actual + 1, data_lines);
                    else levels_DownloadFileCompleted("pass", new AsyncCompletedEventArgs(null, false, null));
                }
                catch { levels_DownloadFileCompleted("pass", new AsyncCompletedEventArgs(null, false, null)); }
            }
        });
    }
    #endregion

    #region General
    public static bool CheckVersionCompatibility(string version) { return CheckVersionCompatibility(version, Application.version); }
    public static bool CheckVersionCompatibility(string version, string app_version)
    {
        string versionParameter = System.Text.RegularExpressions.Regex.Replace(version, "[0-9\\.]", "");
        string[] versionNumberG = version.Replace(versionParameter.ToString(), "").Split(new string[] { "." }, StringSplitOptions.None);
        string[] appVersionG = app_version.Split(new string[] { "." }, StringSplitOptions.None);

        bool versionCompatibility = false;
        for (int i = 0; (i < versionNumberG.Length | i < appVersionG.Length) & !versionCompatibility; i++)
        {
            float versionNumber = 0;
            if (versionNumberG.Length > i) versionNumber = float.Parse(versionNumberG[i]);
            float appVersion = 1;
            if (appVersionG.Length > i) appVersion = float.Parse(appVersionG[i]);

            bool wait = false;

            if (versionParameter.Contains(">") & appVersion > versionNumber) versionCompatibility = true;
            else if (versionParameter.Contains(">") & appVersion > versionNumber) wait = true;
            if (versionParameter.Contains("=") & appVersion == versionNumber & (i >= versionNumberG.Length - 1 & i >= appVersionG.Length - 1)) versionCompatibility = true;
            else if (versionParameter.Contains("=") & appVersion == versionNumber) wait = true;
            if (versionParameter.Contains("<") & appVersion < versionNumber) versionCompatibility = true;
            else if (versionParameter.Contains("<") & appVersion < versionNumber) wait = true;

            if (wait) versionCompatibility = false;
            else if (!versionCompatibility) { versionCompatibility = false; i = versionNumberG.Length; }
        }

        return versionCompatibility;
    }

    public static int NbChiffreEntier(double d) { return Math.Round(d, 0).ToString().Length; }
    #endregion
}
