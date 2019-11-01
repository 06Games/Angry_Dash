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
    private readonly static string passwordKey = "VWtjNGJVbFhTbmhXVkRCNlQwVjBXVTFqUzI1VVZsSlJaREZKTVZkclVXOUtNbFV4VkVad01Fa3lRbTVsYlVWdVdsZEdSbEZYZUdaWGJURkRZbFJOYWxkVlJqaE9WamxKWW10dmVWTnpTM2RQUTA1MVYydFdSVXN3V2taVlYxcHNXVmRTTTB4SVpHbGlhMGx1VldwR1YxQjZXRVJ4VjFFd1RVWnZNVTVFUmpOak1sSTBaV3BGZDNkeVFYSlBWR1pFZFZVdlEzTk5UelZpVFV0dllUSjNjbmR4WnpkM2NXaE1VREZTTUZacldraE9WV1JYVGxSVVJIRkVWVEZOUkZVMFQwUmpNVTVFVGtsV1IxcGFWa2h1UkhGRE1HbDNOa1JFYjAxUGNFdFRURVJ4UjNoMFNWTnlSSFJIZEhOM04ydHhkemRTZDNjMlprUnZUVTl2VVRKYWVXUnRjSEpqTTJNM1RFUkZlVnB0YUhKUGFVVnNkM0pXYzJGWWJEQmthM0Iy";
    private readonly static string apiUrl = "https://06games.ddns.net/accounts/api/";

    public static string Token { get; private set; }
    public static string Username { get; private set; }

    #region API
    public static void CheckAccountFile(Action<bool, string> complete)
    {
        if (Token != null) complete(true, "");
        else if(!InternetAPI.IsConnected()) complete(true, "");
        else if (File.Exists(accountFile))
        {
            RootElement xml = new RootElement(null);
            try { xml = new XML(File.ReadAllText(accountFile)).RootElement; } catch { }
            string provider = xml.GetItem("provider").Value;
            var auth = xml.GetItem("auth");
            foreach (var param in auth) param.Value = Security.Encrypting.Decrypt(param.Value, passwordKey);

            if (provider == "06Games")
            {
                string id = auth.GetItem("id").Value;
                string password = auth.GetItem("password").Value;
                FindObjectOfType<MonoBehaviour>().StartCoroutine(ContactServer($"{apiUrl}auth/connectAccount.php?id={id}&password={password}", complete));
            }
            else if (provider == "Google")
            {
                string token = "";
#if UNITY_ANDROID
                try { token = GooglePlayGames.PlayGamesPlatform.Instance.GetIdToken(); }
                catch { }
#endif
                if (!string.IsNullOrEmpty(token)) FindObjectOfType<MonoBehaviour>().StartCoroutine(ContactServer($"{apiUrl}auth/connectGoogle.php?token={token}", complete));
                else complete(false, "");
            }
            else complete(false, "");
        }
        else complete(false, "");
    }

    static RootElement stateTranslations;
    static UnityWebRequest serverRequest;
    static System.Collections.IEnumerator ContactServer(string url, Action<bool, string> complete)
    {
        bool aborted = false;
        if (stateTranslations == null)
        {
            serverRequest = UnityWebRequest.Get(apiUrl + "lang/" + AngryDash.Language.LangueAPI.selectedLanguage + ".xml");
            yield return serverRequest.SendWebRequest();
            try { stateTranslations = new XML(serverRequest.downloadHandler.text).RootElement; } catch { stateTranslations = new RootElement(null); }
            aborted = serverRequest.error == "Request aborted";
            serverRequest.Dispose();
        }

        if (!aborted)
        {
            serverRequest = UnityWebRequest.Get(url);
            yield return serverRequest.SendWebRequest();
            if (string.IsNullOrEmpty(serverRequest.error))
            {
                var response = new FileFormat.JSON(serverRequest.downloadHandler.text);
                Token = response.Value<string>("token");
                Username = response.Value<string>("username");

                string message = stateTranslations.GetItemByAttribute("string", "text", response.Value<string>("state")).Value;
                if (message == null) message = AngryDash.Language.LangueAPI.Get("native", "account.unkownError", " An unknown error has occurred, please contact a manager and report the following error:\n<i><color=#B40404>[0]</color></i>", response.Value<string>("state"));
                complete.Invoke(response.Value<string>("state") == "Connection succesful !", message.Format());
            }
            else complete(false, serverRequest.error != "Request aborted" ? serverRequest.error : "");
            serverRequest.Dispose();
        }
        else complete(false, "");
    }

    public static void Disconnect()
    {
        File.WriteAllText(accountFile, "");
        FindObjectOfType<LoadingScreenControl>().LoadScreen("Start", new string[] { "Account", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name });
    }

    static void Save(string provider, Dictionary<string, string> auths)
    {
        var xml = new XML().CreateRootElement("account");
        xml.CreateItem("provider").Value = provider;

        var xmlAuth = xml.CreateItem("auth");
        foreach (var auth in auths) xmlAuth.CreateItem(auth.Key).Value = Security.Encrypting.Encrypt(auth.Value == null ? "" : auth.Value, passwordKey);

        File.WriteAllText(accountFile, xml.xmlFile.ToString(false));
    }
    #endregion

    #region Scene
    public event BetterEventHandler complete;
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

                transform.GetChild(1).gameObject.SetActive(true);
                CheckAccountFile(Complete);
            }
        }
    }
    public void Initialize()
    {
        transform.GetChild(1).gameObject.SetActive(true);
        CheckAccountFile(Complete);
    }
    public void CancelServerRequest() { serverRequest.Abort(); }

    void Complete(bool success, string message)
    {
        if (success) Logging.Log("Successful connection to 06Games account", LogType.Log);
        else Logging.Log("06Games account connection failure\n" + message, LogType.Warning);
        Transform connectPanel = transform.GetChild(0);
        connectPanel.gameObject.SetActive(!success);

        connectPanel.GetChild(1).gameObject.SetActive(!success);
        connectPanel.GetChild(1).GetComponent<Text>().text = message;

        transform.GetChild(1).gameObject.SetActive(false);
        if (success) complete.Invoke(null, null);
    }

    public void SignIn_06Games()
    {
        Transform go = transform.GetChild(0).Find("06Games");
        string id = go.GetChild(1).GetComponent<InputField>().text;
        string password = go.GetChild(2).GetComponent<InputField>().text;

        transform.GetChild(1).gameObject.SetActive(true);
        StartCoroutine(ContactServer($"{apiUrl}auth/connectAccount.php?id={id}&password={password}", Complete));
        Save("06Games", new Dictionary<string, string>() { { "id", id }, { "password", password } });
    }
    public void SignIn_Google()
    {
        string token = "";
#if UNITY_ANDROID
        try { token = GooglePlayGames.PlayGamesPlatform.Instance.GetIdToken(); }
        catch { }
#endif
        transform.GetChild(1).gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(token)) StartCoroutine(ContactServer($"{apiUrl}auth/connectGoogle.php?token={token}", Complete));
        else Complete(false, "");
        Save("Google", new Dictionary<string, string>());
    }

    public void Skip() { complete(null, null); }
    #endregion
}
