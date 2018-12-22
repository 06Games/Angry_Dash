using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class Profile : MonoBehaviour
{

    public int menu;
    public Account account;

    Transform Main;
    Transform _Menu;
    Transform Categorie;
    Transform LevelContent;
    Transform LevelDeleteWarning;

    public Selectable[] Default;

    void Start() { Open(false); }
    public void Open(bool open)
    {
        LevelDeleteWarning = transform.GetChild(0).GetChild(1);
        Main = transform.GetChild(0).GetChild(0);
        _Menu = Main.GetChild(2);
        Categorie = Main.GetChild(3);
        LevelContent = Categorie.GetChild(0).GetChild(0).GetChild(0).GetChild(0);

        transform.GetChild(0).gameObject.SetActive(open);
        Main.gameObject.SetActive(open);

        if (open)
        {
            Main.GetChild(1).GetComponent<Text>().text = ConfigAPI.GetString("Account.Username");
            Menu(menu);
        }
    }

    public void Menu(int i) { menu = i; Menu(); }
    void Menu()
    {
        for (int i = 0; i < _Menu.childCount; i++)
        {
            _Menu.GetChild(i).GetComponent<Button>().interactable = i != menu;
            Navigation nav = _Menu.GetChild(i).GetComponent<Button>().navigation;
            nav.selectOnDown = Default[menu];
            _Menu.GetChild(i).GetComponent<Button>().navigation = nav;
        }

        for (int i = 0; i < Categorie.childCount; i++)
            Categorie.GetChild(i).gameObject.SetActive(i == menu);

        if (menu == 0)
            LevelInitialise();
    }

    #region MyLevel
    public string[] lvlNames = new string[0];

    void LevelInitialise(bool force = false)
    {
        if (lvlNames.Length == 0 | force)
        {
            string key = ConfigAPI.GetString("Account.Username") + "/";
            string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/index.php?key=" + key;
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            string Result = "";
            try { Result = client.DownloadString(URL.Replace(" ", "%20")); } catch { }

            string[] files = Result.Split(new string[1] { "<BR />" }, System.StringSplitOptions.None);
            int length = files.Length;
            if (length > 0)
            {
                if (files[length - 1] == "")
                    length = length - 1;
            }

            lvlNames = new string[length];

            for (int i = 0; i < length; i++)
            {
                if (!string.IsNullOrEmpty(files[i]))
                {
                    string[] file = new string[1] { files[i] };
                    if (files[i].Contains(" ; "))
                        file = files[i].Split(new string[1] { " ; " }, System.StringSplitOptions.None);

                    for (int l = 0; l < file.Length; l++)
                    {
                        string[] line = file[l].Split(new string[1] { " = " }, System.StringSplitOptions.None);
                        if (line[0] == "level")
                            lvlNames[i] = file[l].Replace("level = ", "");
                    }
                }
            }
        }

        for (int i = 1; i < LevelContent.childCount; i++)
            Destroy(LevelContent.GetChild(i).gameObject);
        LevelContent.GetChild(0).gameObject.SetActive(false);
        for (int i = 0; i < lvlNames.Length; i++)
        {
            Transform go = Instantiate(LevelContent.GetChild(0).gameObject, LevelContent).transform;
            go.GetChild(0).GetComponent<Text>().text = lvlNames[i];
            int actual = i;
            go.GetChild(1).GetComponent<Button>().onClick.AddListener(() => DeleteWarning(actual));
            go.GetChild(2).GetComponent<Button>().onClick.AddListener(() => Download(actual));
            go.GetChild(2).GetComponent<Button>().interactable = !File.Exists(Application.persistentDataPath + "/Levels/Edited Levels/" + lvlNames[i] + ".level");
            go.GetChild(3).GetComponent<Button>().onClick.AddListener(() => Collaborate(actual));

            go.name = lvlNames[i];
            go.gameObject.SetActive(true);

            if (i == 0)
            {
                Default[0] = go.GetChild(2).GetComponent<Button>();

                for (int v = 0; v < _Menu.childCount; v++)
                {
                    Navigation nav = _Menu.GetChild(v).GetComponent<Button>().navigation;
                    nav.selectOnDown = Default[menu];
                    _Menu.GetChild(v).GetComponent<Button>().navigation = nav;
                }
            }
        }
    }

    public void Collaborate(int levelNumber)
    {
        if (lvlNames.Length > levelNumber)
        {
            Debug.LogWarning("Currently, collaborations aren't available");
        }
    }
    public void Download(int levelNumber)
    {
        if (lvlNames.Length > levelNumber)
        {
            string url = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/files/" + ConfigAPI.GetString("Account.Username") + "/" + lvlNames[levelNumber] + ".level";
            string path = Application.persistentDataPath + "/Levels/Edited Levels/" + lvlNames[levelNumber] + ".level";
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            
            string file = "";
            try { file = client.DownloadString(url.Replace(" ", "%20")); } catch { }
            if (file != "")
            {
                File.WriteAllText(path, file);
                Menu(0);
            }
            else if (Application.isEditor) Debug.LogWarning("Failed to download :\n" + url);
        }
    }
    public void DeleteWarning(int levelNumber)
    {
        if (lvlNames.Length > levelNumber)
        {
            LevelDeleteWarning.GetChild(1).GetComponent<Text>().text = LangueAPI.StringWithArgument("native", "profileMyLevelsDeleteWarningText", lvlNames[levelNumber]);
            LevelDeleteWarning.GetChild(2).GetComponent<Button>().onClick.RemoveAllListeners();
            LevelDeleteWarning.GetChild(2).GetComponent<Button>().onClick.AddListener(() => Delete(levelNumber));
            LevelDeleteWarning.gameObject.SetActive(true);
        }
    }
    public void Delete(int levelNumber)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        string accPath = Application.persistentDataPath.Replace("Angry Dash", "06Games Launcher/") + "account.account";
#elif UNITY_ANDROID
        string accPath = Application.persistentDataPath.Replace("AngryDash", "Launcher") + "/account.account";
#endif
        string[] details = File.ReadAllLines(accPath);
        string id = details[0].Replace("1 = ", "");
        string mdp = details[1].Replace("2 = ", "");
        string url = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/delete.php?id=" + id + "&mdp=" + mdp + "&level=" + lvlNames[levelNumber];
        WebClient client = new WebClient();
        client.Encoding = System.Text.Encoding.UTF8;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        
        string file = "";
        try { file = client.DownloadString(url.Replace(" ", "%20")); } catch { }
        if(file.Contains("Sucess"))
        {
            LevelInitialise(true);
            LevelDeleteWarning.gameObject.SetActive(false);
            GameObject.Find("Main Camera").GetComponent<BaseControl>().SelectButton(Default[menu]);
        }
    }
    #endregion

    #region MyAccount
    public void Disconnect()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        string path = Application.persistentDataPath.Replace("Angry Dash", "06Games Launcher/") + "account.account";
#elif UNITY_ANDROID
        string path = Application.persistentDataPath.Replace("AngryDash", "Launcher") + "/account.account";
#endif
        System.IO.File.WriteAllLines(path, new string[2] { "1 = ", "2 = " });
        System.IO.File.Delete(Application.temporaryCachePath + "/ac.txt");
        ConfigAPI.SetString("Account.Username", "");
        account.Connect("", "", true, false);
    }
    #endregion
}
