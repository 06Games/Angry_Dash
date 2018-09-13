using System;
using System.Collections;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class LangueSelector : MonoBehaviour
{

    public Transform Langues;
    public GameObject RestartRequire;
    public LoadingScreenControl LS;
    public Transform DownloadingFilesPanel;
    public DependenciesManager dependenciesManager;

    public bool AutomatiqueUpdate;

    public string[] LangueDispo;
    public int actuel = 0;
    Sprite[] LangFlag;

    void Start() { NewStart(); transform.GetChild(0).gameObject.SetActive(false); }

    void NewStart()
    {
        if (string.IsNullOrEmpty(LangueAPI.LangGet()))
            ReloadScene();

        if (AutomatiqueUpdate)
        {
            string title = "";
            if (Directory.GetFiles(Application.persistentDataPath + "/Languages/").Length > 1)
                title = LangueAPI.String("downloadLangTitleUpdate");
            else title = LangueAPI.String("downloadLangTitle");
            if (!string.IsNullOrEmpty(title)) DownloadingFilesPanel.GetChild(2).GetComponent<Text>().text = title;
            StartCoroutine(LangueAPI.UpdateFiles(DownloadingFilesPanel, this));
        }
        if (Langues != null)
            if (Langues.childCount <= 1)
                StartCoroutine(GetLangDispo(AutomatiqueUpdate));

        for (int i = 0; i < LangueDispo.Length; i++)
        {
            if (LangueAPI.LangGet() == LangueDispo[i])
                actuel = i;
        }
    }

    void Update()
    {
        if (Langues != null & LangFlag != null)
        {
            for (int i = 0; i < LangueDispo.Length & i < Langues.childCount - 1; i++)
            {
                if (i == actuel)
                    Langues.GetChild(i + 1).GetChild(0).gameObject.SetActive(true);
                else Langues.GetChild(i + 1).GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    public void Chang(int i)
    {
        if (i != actuel)
        {
            actuel = i;
            if (Langues != null)
                RestartRequire.SetActive(true);
        }
    }
    public void Cancel() { RestartRequire.SetActive(false); NewStart(); }
    public void ReloadScene() { Apply(); LS.LoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name); }
    void Apply() { if (LangueDispo.Length > actuel) LangueAPI.LangSet(LangueDispo[actuel]); else Debug.LogError((actuel + 1) + " is more than the number of language file : " + LangueDispo.Length); }

    IEnumerator GetLangDispo(bool forceUpdate = false)
    {
        string[] All = new string[0];
        if (InternetAPI.IsConnected())
        {
            WWW www = new WWW("https://raw.githubusercontent.com/06-Games/Angry-Dash/master/Langues/index");
            yield return www;
            All = www.text.Split(new string[] { "\n" }, StringSplitOptions.None);
        }

        if (All.Length == 0)
        {
            string[] langFiles = Directory.GetFiles(Application.persistentDataPath + "/Languages/");
            All = new string[langFiles.Length];

            for (int i = 0; i < All.Length; i++)
            {
                string[] pathToFile = langFiles[i].Split(new string[2] { "/", "\\" }, StringSplitOptions.None);
                string flag = langFiles[i].Replace("/Languages/", "/Languages/Flags/").Replace(".lang", ".png");
                if (!File.Exists(flag)) flag = "";
                All[i] = pathToFile[pathToFile.Length - 1].Replace(".lang", "") + "[" + langFiles[0] + "][" + flag + "]";
            }
        }

        int lines = All.Length;
        if (string.IsNullOrEmpty(All[lines - 1]))
            lines = lines - 1;

        LangueDispo = new string[lines];
        LangFlag = new Sprite[lines];
        for (int dispo = 0; dispo < lines; dispo++)
        {
            string[] param = All[dispo].Split(new string[] { "[" }, StringSplitOptions.None);
            LangueDispo[dispo] = param[0];
            if (!File.Exists(Application.persistentDataPath + "/Languages/Flags/" + param[0] + ".png") | forceUpdate)
            {
                if (!Directory.Exists(Application.persistentDataPath + "/Languages/Flags/"))
                    Directory.CreateDirectory(Application.persistentDataPath + "/Languages/Flags/");

                WebClient client = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                if (param.Length > 2)
                    if (param[2] != "]")
                        client.DownloadFile(param[2].Replace("]", ""), Application.persistentDataPath + "/Languages/Flags/" + param[0] + ".png");
            }

            Transform go = Instantiate(Langues.GetChild(0).gameObject, new Vector3(), new Quaternion(), Langues).transform;
            go.name = LangueDispo[dispo];
            int i = dispo;
            go.GetComponent<Button>().onClick.RemoveAllListeners();
            go.GetComponent<Button>().onClick.AddListener(() => Chang(i));

            if (File.Exists(Application.persistentDataPath + "/Languages/Flags/" + param[0] + ".png"))
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/Languages/Flags/" + param[0] + ".png"));
                LangFlag[dispo] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            }
            else LangFlag[dispo] = Langues.GetChild(0).GetComponent<Image>().sprite;
            go.GetComponent<Image>().sprite = LangFlag[dispo];
            go.gameObject.SetActive(true);
        }

        int paddingX = (int)((Langues.GetComponent<RectTransform>().sizeDelta.x - (167 * lines)) / 2); ;
        Langues.GetComponent<HorizontalLayoutGroup>().padding.left = paddingX;
        Langues.GetComponent<HorizontalLayoutGroup>().padding.right = paddingX;

        if (LangueAPI.LangGet() == null)
            Apply();

        for (int i = 0; i < LangueDispo.Length; i++)
        {
            if (LangueAPI.LangGet() == LangueDispo[i])
                actuel = i;
        }
    }

    public void End()
    {
        UnityThread.executeInUpdate(() =>
        {
            DownloadingFilesPanel.gameObject.SetActive(false);
            if (dependenciesManager != null)
                dependenciesManager.DownloadAllRequiredTex();
        });
    }
}
