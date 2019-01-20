using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class Account : MonoBehaviour
{

    void Start()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(0).GetChild(4).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);

#if UNITY_EDITOR || UNITY_STANDALONE
        string path = Application.persistentDataPath.Replace("Angry Dash", "06Games Launcher/") + "account.account";
#elif UNITY_ANDROID
        string path = Application.persistentDataPath.Replace("AngryDash", "Launcher") + "/account.account";
#endif
        if (InternetAPI.IsConnected())
        {
            if (File.Exists(path))
            {
                try
                {
                    string[] details = File.ReadAllLines(path);
                    if (!File.Exists(Application.temporaryCachePath + "/ac.txt"))
                        Connect(details[0].Replace("1 = ", ""), details[1].Replace("2 = ", ""), true);
                    else if (Security.Encrypting.Decrypt(File.ReadAllLines(Application.temporaryCachePath + "/ac.txt")[0], details[1].Replace("2 = ", "")) != details[0].Replace("1 = ", "") + Logging.pathToLogFile)
                        Connect(details[0].Replace("1 = ", ""), details[1].Replace("2 = ", ""), true);
                }
                catch
                {
                    if (!Directory.Exists(path.Replace("account.account", "")))
                        Directory.CreateDirectory(path.Replace("account.account", ""));
                    transform.GetChild(0).gameObject.SetActive(true);
                }
            }
            else
            {
                if (!Directory.Exists(path.Replace("account.account", "")))
                    Directory.CreateDirectory(path.Replace("account.account", ""));
                transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    public void ConnectBtn()
    {
        transform.GetChild(0).GetChild(4).gameObject.SetActive(false);
        Connect(transform.GetChild(0).GetChild(1).GetComponent<InputField>().text, transform.GetChild(0).GetChild(2).GetComponent<InputField>().text, false);
    }
    public bool Connect(string user, string mdp, bool hash) { return Connect(user, mdp, hash, true); }
    public bool Connect(string user, string mdp, bool hash, bool showErrors = true)
    { 
        string MDP = mdp;
        if (!hash)
            MDP = Security.Hashing.SHA384(mdp);

        string url = "https://06games.ddns.net/accounts/lite/connect.php?id=" + user + "&mdp=" + MDP + "&hash=true";
        WebClient client = new WebClient();
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        string Result = "";
        try
        {
            Result = client.DownloadString(url);
        }
        catch
        {
#if UNITY_EDITOR
            Result = "Connection succesful !<br><br>Unity<br>UnityAccount<br>01/01/2000<br><b>Jeux :</b><dd>";
#endif
        }

        if (Result.Contains("Connection succesful !"))
        {
            Logging.Log("Successful connection to 06Games account", LogType.Log);
            transform.GetChild(0).gameObject.SetActive(false);

#if UNITY_EDITOR || UNITY_STANDALONE
            string path = Application.persistentDataPath.Replace("Angry Dash", "06Games Launcher/") + "account.account";
#elif UNITY_ANDROID
        string path = Application.persistentDataPath.Replace("AngryDash", "Launcher") + "/account.account";
#endif
            string[] a = Result.Split(new string[] { "<br>" }, StringSplitOptions.None);
            ConfigAPI.SetString("Account.Username", a[3]);
            File.WriteAllLines(path, new string[2] { "1 = " + user, "2 = " + MDP });
            File.WriteAllLines(Application.temporaryCachePath + "/ac.txt", new string[1] { Security.Encrypting.Encrypt(user + Logging.pathToLogFile, mdp) });
        }
        else
        {
                if(showErrors) Logging.Log("06Games account connection failure", LogType.Warning);
            transform.GetChild(0).gameObject.SetActive(true);
            if(!hash)
                transform.GetChild(0).GetChild(4).gameObject.SetActive(true);
        }

        transform.GetChild(1).gameObject.SetActive(false);
        return Result.Contains("Connection succesful !");
    }
}
