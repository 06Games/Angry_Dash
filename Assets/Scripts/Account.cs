using FileFormat.XML;
using System;
using System.Collections.Generic;
using System.IO;
using Tools;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Account : MonoBehaviour
{
    public static string accountFile
    {
        get
        {
#if UNITY_IOS && !UNITY_EDITOR
            return Application.persistentDataPath + "/account.xml";
#else
            return Application.persistentDataPath + "/../account.xml";
#endif
        }
    }
    const string passwordKey = "VWtjNGJVbFhTbmhXVkRCNlQwVjBXVTFqUzI1VVZsSlJaREZKTVZkclVXOUtNbFV4VkVad01Fa3lRbTVsYlVWdVdsZEdSbEZYZUdaWGJURkRZbFJOYWxkVlJqaE9WamxKWW10dmVWTnpTM2RQUTA1MVYydFdSVXN3V2taVlYxcHNXVmRTTTB4SVpHbGlhMGx1VldwR1YxQjZXRVJ4VjFFd1RVWnZNVTVFUmpOak1sSTBaV3BGZDNkeVFYSlBWR1pFZFZVdlEzTk5UelZpVFV0dllUSjNjbmR4WnpkM2NXaE1VREZTTUZacldraE9WV1JYVGxSVVJIRkVWVEZOUkZVMFQwUmpNVTVFVGtsV1IxcGFWa2h1UkhGRE1HbDNOa1JFYjAxUGNFdFRURVJ4UjNoMFNWTnlSSFJIZEhOM04ydHhkemRTZDNjMlprUnZUVTl2VVRKYWVXUnRjSEpqTTJNM1RFUkZlVnB0YUhKUGFVVnNkM0pXYzJGWWJEQmthM0Iy";
    private readonly string apiUrl = "https://06games.ddns.net/accounts/api/";

    public static string Token { get; private set; }
    public static string Username { get; private set; } = "EvanG";
    [Obsolete("Tokens are better ! No more working !")] public static string Password
    {
#if UNITY_EDITOR
        get
        {
            RootElement root = new RootElement(null);
            try { root = new XML(File.ReadAllText(accountFile)).RootElement; } catch { }
            return Security.Encrypting.Decrypt(root.GetItem("password").Value, passwordKey);
        }
        set { }
#else
        get; private set;
#endif
    }

    public bool logErrors { get; set; } = true;
    public event BetterEventHandler complete;
    public void CheckAccountFile(Action<bool, string> complete)
    {
        if (File.Exists(accountFile))
        {
            RootElement xml = new RootElement(null);
            try { xml = new XML(File.ReadAllText(accountFile)).RootElement; } catch { }
            string provider = xml.GetItem("provider").Value;
            var auth = xml.GetItem("auth");

            if (provider == "06Games")
            {
                string id = auth.GetItem("id").Value;
                string password = auth.GetItem("password").Value;
                StartCoroutine(ContactServer($"{apiUrl}auth/connectAccount.php?id={id}&password={password}", complete));
            }
            else if (provider == "Google")
            {
                string token = ((GooglePlayGames.PlayGamesLocalUser)UnityEngine.Social.localUser).GetIdToken();
                StartCoroutine(ContactServer($"{apiUrl}auth/connectGoogle.php?token={token}", Complete));
                Save("Google", new Dictionary<string, string>());
            }
            else complete(false, "");
        }
        else complete(false, "");
    }
    System.Collections.IEnumerator ContactServer(string url, Action<bool, string> complete)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();
            var response = new FileFormat.JSON(webRequest.downloadHandler.text);
            Token = response.Value<string>("token");
            Username = response.Value<string>("username");
            complete.Invoke(response.Value<string>("state") == "Connection succesful !", response.Value<string>("state"));
        }
    }

    public static void Disconnect()
    {
        File.WriteAllText(accountFile, "");
        FindObjectOfType<LoadingScreenControl>().LoadScreen("Start", new string[] { "Account", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name });
    }



    void Start()
    {
        LoadingScreenControl LSC = FindObjectOfType<LoadingScreenControl>();
        string[] args = LSC.GetArgs();
        if (args == null) return;
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
                CheckAccountFile(Complete);
            }
        }
    }
    public void Initialize() { CheckAccountFile(Complete); }

    void Complete(bool success, string message)
    {
        if(success) Logging.Log("Successful connection to 06Games account", LogType.Log);
        else Logging.Log("06Games account connection failure\n" + message, LogType.Warning);
        Transform connectPanel = transform.GetChild(0);
        connectPanel.gameObject.SetActive(!success);

        connectPanel.GetChild(1).gameObject.SetActive(!success);
        connectPanel.GetChild(1).GetComponent<Text>().text = message;

        if (success) complete.Invoke(null, null);
    }
    void Save(string provider, Dictionary<string, string> auths)
    {
        var xml = new XML().CreateRootElement("account");
        xml.CreateItem("provider").Value = provider;

        var xmlAuth = xml.CreateItem("auth");
        foreach (var auth in auths) xmlAuth.CreateItem(auth.Key).Value = auth.Value;

        File.WriteAllText(accountFile, xml.xmlFile.ToString(false));
    }

    public void SignIn_06Games()
    {
        Transform go = transform.GetChild(0).Find("06Games");
        string id = go.GetChild(1).GetComponent<InputField>().text;
        string password = go.GetChild(2).GetComponent<InputField>().text;
        StartCoroutine(ContactServer($"{apiUrl}auth/connectAccount.php?id={id}&password={password}", Complete));
        Save("06Games", new Dictionary<string, string>() { { "id", id }, { "password", password } });
    }
    public void SignIn_Google()
    {
        string token = ((GooglePlayGames.PlayGamesLocalUser)UnityEngine.Social.localUser).GetIdToken();
        StartCoroutine(ContactServer($"{apiUrl}auth/connectGoogle.php?token={token}", Complete));
        Save("Google", new Dictionary<string, string>());
    }

    public void Skip() { complete(null, null); }
}
