using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class DependenciesManager : MonoBehaviour
{

    GameObject DownloadPanel;
    Slider BigSlider;
    Slider SmallSlider;

    public Social _Social;

    #region Textures
    public void DownloadAllRequiredTex()
    {
        DownloadPanel = transform.GetChild(1).gameObject;
        BigSlider = DownloadPanel.transform.GetChild(1).GetComponent<Slider>();
        SmallSlider = DownloadPanel.transform.GetChild(2).GetComponent<Slider>();

        if (InternetAPI.IsConnected())
        {
            string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/items/";
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string Result = "";
            try { Result = client.DownloadString(URL).Replace("<BR />", "\n"); } catch { wc_DownloadFileCompleted("pass", new AsyncCompletedEventArgs(null, false, null)); return; }
            string[] lines = Result.Split(new string[1] { "\n" }, StringSplitOptions.None);
            int CatNumber = int.Parse(lines[lines.Length - 1].Replace("<dir>", "").Replace("</dir>", ""));

            DownloadPanel.SetActive(true);
            SmallSlider.transform.GetChild(4).gameObject.SetActive(false);

            fileCat = new int[CatNumber];
            for (int i = 0; i < lines.Length - 1; i++)
            {
                int c = int.Parse(lines[i].Split(new string[] { "<name>" }, StringSplitOptions.None)[1].Split(new string[] { "</name>" }, StringSplitOptions.None)[0].Split(new string[] { "/" }, StringSplitOptions.None)[0]);
                fileCat[c]++;
            }

            downloadFile(0, lines, Application.persistentDataPath + "/Textures/", CatNumber);
        }
        else wc_DownloadFileCompleted("pass", new AsyncCompletedEventArgs(null, false, null));
    }

    int[] fileCat = new int[0];
    string[] downData = new string[5] { "", "", "", "", "" };
    Stopwatch sw = new Stopwatch();
    public void downloadFile(int actual, string[] lines, string mainPath, int CatNumber)
    {
        string version = lines[actual].Split(new string[] { "</version>" }, StringSplitOptions.None)[0].Replace("<version>", "");
        string name = lines[actual].Split(new string[] { "<name>" }, StringSplitOptions.None)[1].Split(new string[] { "</name>" }, StringSplitOptions.None)[0];
        string cat = name.Split(new string[] { "/" }, StringSplitOptions.None)[0];
        int size = int.Parse(lines[actual].Split(new string[] { "<size>" }, StringSplitOptions.None)[1].Split(new string[] { "B</size>" }, StringSplitOptions.None)[0]);


        int catSub = 0;
        for (int i = 0; i < fileCat.Length; i++)
        {
            if (i < int.Parse(cat))
                catSub = catSub + fileCat[i];
            else i = fileCat.Length;
        }
        UnityThread.executeInUpdate(() =>
        {
            BigSlider.value = (int.Parse(cat) + 1) * 100 / (float)CatNumber;
            SmallSlider.value = (actual - catSub) / (float)fileCat[int.Parse(cat)];

            BigSlider.transform.GetChild(3).GetComponent<Text>().text = LangueAPI.StringWithArgument("downloadTexType", new string[2] { (int.Parse(cat) + 1).ToString(), CatNumber.ToString() });
            SmallSlider.transform.GetChild(3).GetComponent<Text>().text = LangueAPI.StringWithArgument("downloadTexTexNumber", new string[2] { (actual + 1 - catSub).ToString(), fileCat[int.Parse(cat)].ToString() });
        });

        string desktopPath = mainPath + name;

        bool down = false;
        if (!CheckVersionCompatibility(version)) down = false;
        else if (File.Exists(desktopPath))
            if (new FileInfo(desktopPath).Length != size) down = true;
            else down = false;
        else down = true;
        if (down)
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Textures/" + cat))
                Directory.CreateDirectory(Application.persistentDataPath + "/Textures/" + cat);

            string url = "https://06games.ddns.net/Projects/Games/Angry%20Dash/items/" + name;

            using (WebClient wc = new WebClient())
            {
                sw.Start();
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += wc_DownloadFileCompleted;
                wc.DownloadFileAsync(new Uri(url), desktopPath);
            }
        }
        else
        {
            SmallSlider.transform.GetChild(4).gameObject.SetActive(false);
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
        downData[3] = catSub.ToString();
        downData[4] = CatNumber.ToString();
    }

    private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        // vitesse
        double speed = Math.Round((e.BytesReceived / sw.Elapsed.TotalSeconds), 1);
        int SpeedReduct = 0;
        for (int i = 0; NbChiffreEntier(speed) > 3 & i <= 4; i++)
        {
            speed = speed / 1024d;
            SpeedReduct++;
        }
        speed = Math.Round(speed, 1);
        string SpeedLangueID = "downloadSpeedB";
        if (SpeedReduct == 0) SpeedLangueID = "downloadSpeedB";
        else if (SpeedReduct == 1) SpeedLangueID = "downloadSpeedKB";
        else if (SpeedReduct == 2) SpeedLangueID = "downloadSpeedMB";
        else if (SpeedReduct == 3) SpeedLangueID = "downloadSpeedGB";
        else if (SpeedReduct == 4) SpeedLangueID = "downloadSpeedTB";


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
        string LangueID = "downloadStateB";
        if (Reduct == 0) LangueID = "downloadStateB";
        else if (Reduct == 1) LangueID = "downloadStateKB";
        else if (Reduct == 2) LangueID = "downloadStateMB";
        else if (Reduct == 3) LangueID = "downloadStateGB";
        else if (Reduct == 4) LangueID = "downloadStateTB";

        UnityThread.executeInUpdate(() =>
        {
            string speedText = LangueAPI.StringWithArgument(SpeedLangueID, speed);
            string downloaded = LangueAPI.StringWithArgument(LangueID, new string[] { Actual.ToString("0.0"), Total.ToString("0.0") });
            string pourcent = LangueAPI.StringWithArgument("downloadStatePercentage", pourcentage.ToString("00"));

            Text DownloadInfo = SmallSlider.transform.GetChild(4).GetComponent<Text>();
            DownloadInfo.gameObject.SetActive(true);
            DownloadInfo.text = speedText + " - " + downloaded + " - <color=grey>" + pourcent + "</color>";

            //Progression plus détaillée
            string name = downData[1].Split(new string[1] { "\n" }, StringSplitOptions.None)[int.Parse(downData[0])].Split(new string[] { "<name>" }, StringSplitOptions.None)[1].Split(new string[] { "</name>" }, StringSplitOptions.None)[0];
            string cat = name.Split(new string[] { "/" }, StringSplitOptions.None)[0];
            float baseValue = (int.Parse(downData[0]) - int.Parse(downData[3])) / (float)fileCat[int.Parse(cat)];
            float oneValue = 1 / (float)fileCat[int.Parse(cat)];
            SmallSlider.value = baseValue + ((pourcentage / 100F) * oneValue);
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
                try
                {
                    if (int.Parse(downData[0]) < s.Length - 2)
                        downloadFile(int.Parse(downData[0]) + 1, s, downData[2], int.Parse(downData[4]));
                    else wc_DownloadFileCompleted("pass", new AsyncCompletedEventArgs(null, false, null));
                }
                catch { wc_DownloadFileCompleted("pass", new AsyncCompletedEventArgs(null, false, null)); }
            }
        });
    }
    #endregion

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
                go.transform.GetChild(0).GetComponent<Slider>().value = (actual + 1) / lines.Length;
                go.transform.GetChild(2).GetComponent<Text>().text = actual + "/" + lines.Length;
            });

            string version = lines[actual].Split(new string[] { "</version>" }, StringSplitOptions.None)[0].Replace("<version>", "");
            string name = lines[actual].Split(new string[] { "<name>" }, StringSplitOptions.None)[1].Split(new string[] { "</name>" }, StringSplitOptions.None)[0];
            int size = int.Parse(lines[actual].Split(new string[] { "<size>" }, StringSplitOptions.None)[1].Split(new string[] { "B</size>" }, StringSplitOptions.None)[0]);

            string desktopPath = Application.persistentDataPath + "/Level/Solo/" + name;

            bool down = false;
            if (!CheckVersionCompatibility(version)) down = false;
            else if (File.Exists(desktopPath))
                if (new FileInfo(desktopPath).Length != size) down = true;
                else down = false;
            else down = true;
            if (down)
            {
                if (!Directory.Exists(Application.persistentDataPath + "/Level/Solo/"))
                    Directory.CreateDirectory(Application.persistentDataPath + "/Level/Solo/");

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
                UnityThread.executeInUpdate(() =>
                {
                    transform.GetChild(2).gameObject.SetActive(false);
                    _Social.NewStart();
                });
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

    int NbChiffreEntier(double d) { return Math.Round(d, 0).ToString().Length; }
    #endregion
}
