using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using System;
using Tools;

public class Account : MonoBehaviour
{
    public static string accountFile
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return Application.persistentDataPath + "/../06Games Launcher/account.account";
#elif UNITY_ANDROID
            return Application.persistentDataPath + "/../com.fr_06Games.Launcher/account.account";
#endif
        }
    }
    const string passwordKey = "VWtjNGJVbFhTbmhXVkRCNlQwVjBXVTFqUzI1VVZsSlJaREZKTVZkclVXOUtNbFV4VkVad01Fa3lRbTVsYlVWdVdsZEdSbEZYZUdaWGJURkRZbFJOYWxkVlJqaE9WamxKWW10dmVWTnpTM2RQUTA1MVYydFdSVXN3V2taVlYxcHNXVmRTTTB4SVpHbGlhMGx1VldwR1YxQjZXRVJ4VjFFd1RVWnZNVTVFUmpOak1sSTBaV3BGZDNkeVFYSlBWR1pFZFZVdlEzTk5UelZpVFV0dllUSjNjbmR4WnpkM2NXaE1VREZTTUZacldraE9WV1JYVGxSVVJIRkVWVEZOUkZVMFQwUmpNVTVFVGtsV1IxcGFWa2h1UkhGRE1HbDNOa1JFYjAxUGNFdFRURVJ4UjNoMFNWTnlSSFJIZEhOM04ydHhkemRTZDNjMlprUnZUVTl2VVRKYWVXUnRjSEpqTTJNM1RFUkZlVnB0YUhKUGFVVnNkM0pXYzJGWWJEQmthM0Iy";

    public static string Username { get; private set; } = "EvanG";
    public static string Password { get; private set; }

    void Start()
    {
        LoadingScreenControl LSC = FindObjectOfType<LoadingScreenControl>();
        string[] args = LSC.GetArgs();
        if (args.Length >= 2)
        {
            if (args[0] == "Account")
            {
                for (int i = 0; i < transform.parent.childCount; i++)
                {
                    GameObject child = transform.parent.GetChild(i).gameObject;
                    if (child != gameObject) child.SetActive(false);
                }
                complete += (sender, e) => LSC.LoadScreen(args[1]);
                Initialize();
            }
        }
    }

    public bool logErrors { get; set; } = true;
    public event BetterEventHandler complete;
    public void Initialize()
    {
        void NoConnectionInformation()
        {
            transform.GetChild(0).GetChild(3).GetComponent<Button>().onClick.AddListener(() =>
            {
                transform.GetChild(0).GetChild(3).GetComponent<Button>().interactable = false;
                string username = transform.GetChild(0).GetChild(1).GetComponent<InputField>().text;
                string password = transform.GetChild(0).GetChild(2).GetComponent<InputField>().text;
                bool success = Connect(username, password, (sender, e) => complete.Invoke(sender, e));

                if (success)
                {
                    FileInfo accountF = new FileInfo(accountFile);
                    if (!accountF.Directory.Exists) Directory.CreateDirectory(accountF.DirectoryName);
                    File.WriteAllLines(accountF.FullName, new string[] { username, Security.Encrypting.Encrypt(password, passwordKey) });
                }
                else transform.GetChild(0).GetChild(3).GetComponent<Button>().interactable = true;
            });
            transform.GetChild(0).GetChild(4).gameObject.SetActive(false);
            transform.GetChild(0).gameObject.SetActive(true);
        }

        if (InternetAPI.IsConnected())
        {
            if (File.Exists(accountFile))
            {
                string[] details = File.ReadAllLines(accountFile);
                if (details.Length >= 2)
                {
                    string username = details[0];
                    string password = null;
                    try { password = Security.Encrypting.Decrypt(details[1], passwordKey); }
                    catch { NoConnectionInformation(); return; }

                    bool success = Connect(username, password, (sender, e) => complete.Invoke(sender, e));
                    if (!success) NoConnectionInformation();
                }
                else NoConnectionInformation();
            }
            else NoConnectionInformation();
        }
    }

    public static void Disconnect()
    {
        File.WriteAllLines(accountFile, new string[] { });
        FindObjectOfType<LoadingScreenControl>().LoadScreen("Start",
            new string[] { "Account", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name });
    }

    public bool Connect(string username, string password, BetterEventHandler success = null)
    {
        string url = "https://06games.ddns.net/accounts/lite/connect.php?id=" + username + "&mdp=" + password;
        WebClient client = new WebClient();
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        string Result = "";
        try { Result = client.DownloadString(url); }
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

            string[] a = Result.Split(new string[] { "<br>" }, StringSplitOptions.None);
            Username = a[3];
            Password = password;

            if (success != null) success.Invoke(null, null);
        }
        else
        {
            if (logErrors) Logging.Log("06Games account connection failure", LogType.Warning);
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).GetChild(4).gameObject.SetActive(true);


            string errorId = "";
            if (Result == "You must enter a username (or an e-mail address) !") errorId = "AccountErrorNoUsername";
            else if (Result == "This account doesn't exist !") errorId = "AccountErrorUsername";
            else if (Result == "You must enter a password !") errorId = "AccountErrorNoPassword";
            else if (Result == "The password you entered doesn't match the one on our databases !") errorId = "AccountErrorPassword";
            else if (Result == "Bad URL") errorId = "AccountErrorInternal";

            if (!string.IsNullOrEmpty(errorId))
                transform.GetChild(0).GetChild(4).GetComponent<Text>().text = LangueAPI.String("native", errorId, Result);
            else transform.GetChild(0).GetChild(4).GetComponent<Text>().text =
                    LangueAPI.StringWithArgument("native", "AccountErrorUnkown", Result, "[0]");
        }

        return Result.Contains("Connection succesful !");
    }
}
