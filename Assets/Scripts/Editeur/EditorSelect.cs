using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using System.Net;

public class EditorSelect : MonoBehaviour
{
    public Text SongUsed;
    public GameObject Selector;
    public Sprite[] SelectorSp;
    public GameObject Info;
    public GameObject Cam;
    public GameObject _NewG;
    public GameObject UploadPanel;
    InputField[] _New = new InputField[2];
    public Editeur editeur;
    public Soundboard SoundBoard;

    public int SelectedLevel = -1;
    public string[] Desc;
    public string[] file;
    public string[] Songs;
    string[] files;
    int lastItem = -1;

    void Start()
    {
        SoundBoard.RefreshList();
        _New[0] = _NewG.transform.GetChild(0).GetChild(2).gameObject.GetComponent<InputField>();
        _New[1] = _NewG.transform.GetChild(0).GetChild(3).gameObject.GetComponent<InputField>();
        transform.GetChild(0).gameObject.SetActive(true);
        for (int i = 1; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(false);
        NewStart();
    }

    public void NewStart()
    {
        lastItem = -1;
        SelectedLevel = -1;
        _NewG.SetActive(false);

        Cam.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, -10);
        Cam.GetComponent<Camera>().orthographicSize = Screen.height / 2;

        string directory = Application.persistentDataPath + "/Saved Level/";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        files = Directory.GetFiles(directory);
        file = files;
        int Files = file.Length;
        Desc = new string[Files];
        Songs = new string[Files];

        Page(1);
    }

    public void OpenMostRecentlevel()
    {
        string[] fileSort = files;
        DateTime[] creationTimes = new DateTime[fileSort.Length];
        for (int i = 0; i < fileSort.Length; i++)
            creationTimes[i] = new FileInfo(fileSort[i]).CreationTime;
        Array.Sort(creationTimes, fileSort);
    }

    public static string FormatedDate(DateTime DT)
    {
        //string a = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/", "'/'");
        string a = "dd'/'MM'/'yyyy";
        return DT.ToString(a);
    }

    public void ChangLevel(int button)
    {
        if (button != -1)
        {
            SongUsed.transform.parent.gameObject.SetActive(true);
            transform.GetChild(2).GetChild(2).gameObject.SetActive(true);
            Info.SetActive(true);
            Selector.transform.parent.position = new Vector2((Screen.width / 2) - 308.5F, Screen.height / 2);
            Selector.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(617, 1008);
            Selector.GetComponent<Image>().sprite = SelectorSp[0];
            Selector.transform.parent.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(43.5F - (Selector.transform.parent.GetComponent<RectTransform>().sizeDelta.x / 2), -37.5F);
            Selector.transform.parent.GetChild(2).gameObject.SetActive(false);
        }

        SelectedLevel = button + (lastItem - 4);

        for (int i = 1; i < 6; i++)
        {
            if (i == button + 1)
                Selector.transform.GetChild(i).GetComponent<Image>().color = new Color32(80, 80, 80, 255);
            else Selector.transform.GetChild(i).GetComponent<Image>().color = new Color32(95, 95, 95, 255);
        }
        if (button != -1)
        {
            Info.transform.GetChild(1).GetChild(0).gameObject.GetComponent<InputField>().text = Desc[button];

            string[] Name = file[SelectedLevel].Split(new string[] { "/", "\\" }, StringSplitOptions.None);
            string fileName = Name[Name.Length - 1].Replace(".level", "");
            if (fileName != "")
                Info.transform.GetChild(0).GetChild(0).gameObject.GetComponent<InputField>().text = fileName;
            else Info.transform.GetChild(0).GetChild(0).gameObject.GetComponent<InputField>().text = "";

            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string Result = "";
            string fileID = "";
            string[] f = File.ReadAllLines(file[SelectedLevel]);
            int pID = -1;
            for (int x = 0; x < f.Length; x++)
            {
                if (f[x].Contains("publicID = ") & pID == -1)
                    pID = x;
            }
            if (pID != -1)
                fileID = f[pID].Replace("publicID = ", "");
            string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/index.php?key=" + ConfigAPI.GetString("Account.Username") + "/" + fileName + "_" + fileID;
            try { Result = client.DownloadString(URL); } catch { }
            Info.transform.GetChild(0).GetChild(0).gameObject.GetComponent<InputField>().interactable = !Result.Contains("level = " + fileName + " ;");

            SongUsed.text = Songs[button];
        }
        else
        {
            Info.transform.GetChild(1).GetChild(0).gameObject.GetComponent<InputField>().text = "";
            SongUsed.text = "";
        }
    }

    public void ChangDesc(InputField IF)
    { ChangDesc(IF.text); }
    void ChangDesc(string IF)
    {
        if (SelectedLevel != -1)
        {
            Desc[SelectedLevel] = IF;
            string[] a = File.ReadAllLines(file[SelectedLevel]);

            int d = -1;
            for (int x = 0; x < a.Length; x++)
            {
                if (a[x].Contains("description = ") & d == -1)
                    d = x;
            }
            if (d != -1)
                a[d] = "description = " + Desc[SelectedLevel];
            else
            {
                Desc[SelectedLevel] = "";
                IF = "Description incompatible";
            }
            File.WriteAllLines(file[SelectedLevel], a);
        }
    }
    public void ChangName(InputField IF)
    { ChangName(IF.text); }
    void ChangName(string IF)
    {
        if (SelectedLevel != -1 & !string.IsNullOrEmpty(IF))
        {
            if (!File.Exists(Application.persistentDataPath + "/Saved Level/" + IF.ToLower() + ".level"))
            {
                WebClient client = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                string Result = "";

                string[] Name = file[SelectedLevel].Split(new string[] { "/", "\\" }, StringSplitOptions.None);
                string fileName = Name[Name.Length - 1].Replace(".level", "");

                string fileID = "";
                string[] f = File.ReadAllLines(file[SelectedLevel]);
                int pID = -1;
                for (int x = 0; x < f.Length; x++)
                {
                    if (f[x].Contains("publicID = ") & pID == -1)
                        pID = x;
                }
                if (pID != -1)
                    fileID = f[pID].Replace("publicID = ", "");

                string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/index.php?key=" + ConfigAPI.GetString("Account.Username") + "/" + fileName + "_" + fileID;
                try { Result = client.DownloadString(URL); } catch { }
                if (Result.Contains("level = " + fileName + " ;")) return;


                string newFile = Application.persistentDataPath + "/Saved Level/" + IF + ".level";
                File.Move(file[SelectedLevel], newFile);
                file[SelectedLevel] = newFile;
                Page(0);

                ChangLevel(4 - (lastItem - SelectedLevel));
            }
        }
    }

    public void Play()
    {
        if (GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
        editeur.EditFile(file[SelectedLevel]);
        Recent.LvlPlayed(file[SelectedLevel], true);
    }

    public void Copy()
    {
        File.Copy(file[SelectedLevel], file[SelectedLevel].Replace(".level", " - Copy.level"));
        NewStart();
    }

    public void Del()
    {
        File.Delete(file[SelectedLevel]);
        NewStart();
        ChangLevel(-1);
    }

    public void New()
    {
        bool n = File.Exists(Application.persistentDataPath + "/Saved Level/" + _New[0].text.ToLower() + ".level");

        if (!n)
        {
            editeur.CreateFile(_New[0].text.ToLower(), Application.persistentDataPath + "/Saved Level/", _New[1].text);

            if (GameObject.Find("Audio") != null)
                GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
        }
        else CheckNewLevelName(_New[0]);
    }
    public void CheckNewLevelName(InputField IF)
    {
        Image i = IF.transform.GetChild(3).gameObject.GetComponent<Image>();
        if (File.Exists(Application.persistentDataPath + "/Saved Level/" + IF.text.ToLower() + ".level") | IF.text == "" | IF.text.Contains(".level"))
            i.color = new Color32(163, 0, 0, 255);
        else i.color = new Color32(129, 129, 129, 255);
    }

    public void Page(int v)
    {
        SongUsed.transform.parent.gameObject.SetActive(false);
        transform.GetChild(2).GetChild(2).gameObject.SetActive(false);
        Info.SetActive(false);
        Selector.transform.parent.position = new Vector2(Screen.width / 2, Screen.height / 2);
        Selector.transform.parent.GetComponent<RectTransform>().sizeDelta = new Vector2(1234, 1008);
        Selector.GetComponent<Image>().sprite = SelectorSp[1];
        Selector.transform.parent.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(-250, -37.5F);
        Selector.transform.parent.GetChild(2).gameObject.SetActive(true);

        int f = lastItem + 1;

        if (v == -1)
            f = lastItem - 9;
        else if (v == 0)
            f = lastItem - 4;

        lastItem = f + 4;

        for (int i = 0; i < 5; i++)
        {
            Transform go = Selector.transform.GetChild(i + 1);

            if (file.Length > f)
            {
                go.gameObject.SetActive(true);

                string[] Name = file[f].Split(new string[] { "/", "\\" }, StringSplitOptions.None);
                DateTime UTC = File.GetLastWriteTime(file[i]);

                go.GetChild(0).GetComponent<Text>().text = Name[Name.Length - 1].Replace(".level", "");
                go.GetChild(1).GetComponent<Text>().text = FormatedDate(UTC);

                string[] a = File.ReadAllLines(file[f]);
                int d = -1;
                for (int x = 0; x < a.Length; x++)
                {
                    if (a[x].Contains("description = ") & d == -1)
                        d = x;
                }
                if (d == -1)
                    Desc[i] = "Description incompatible";
                else Desc[i] = a[d].Replace("description = ", "");

                int m = -1;
                for (int x = 0; x < a.Length; x++)
                {
                    if (a[x].Contains("music = ") & m == -1)
                        m = x;
                }
                if (m == -1)
                    Songs[i] = "No Music";
                else if (a[m].Replace("music = ", "") == "")
                    Songs[i] = "No Music";
                else if (!File.Exists(Application.persistentDataPath + "/Musics/" + a[m].Replace("music = ", "")))
                    Songs[i] = "Unkown Music";
                else
                {
                    String AT = "audio/x-wav";
                    if (Soundboard.NativeFileFormat() == AudioType.OGGVORBIS)
                        AT = "application/ogg";
                    else if (Soundboard.NativeFileFormat() == AudioType.MPEG)
                        AT = "audio/mpeg";
                    Songs[i] = TagLib.File.Create(Application.persistentDataPath + "/Musics/" + a[m].Replace("music = ", ""), AT, TagLib.ReadStyle.None).Tag.Title;
                }
                f = f + 1;
            }
            else go.gameObject.SetActive(false);
        }

        Selector.transform.GetChild(6).GetComponent<Button>().interactable = lastItem < file.Length - 1;
        Selector.transform.GetChild(0).GetComponent<Button>().interactable = lastItem - 5 > -1;
        for (int i = 1; i < 6; i++)
            Selector.transform.GetChild(i).GetComponent<Image>().color = new Color32(95, 95, 95, 255);
    }
    public void Search(InputField IF)
    {
        lastItem = 4;
        SelectedLevel = -1;
        _NewG.SetActive(false);

        if (string.IsNullOrEmpty(IF.text))
            NewStart();
        else
        {
            bool[] fileList = new bool[files.Length];
            if (IF != null)
            {
                if (!string.IsNullOrEmpty(IF.text))
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        string[] file = files[i].Split(new string[1] { "/" }, System.StringSplitOptions.None);
                        string fileName = file[file.Length - 1].Replace(".level", "");
                        fileList[i] = fileName.ToLower().Contains(IF.text.ToLower());
                    }
                }
            }

            int item = 0;
            string[] fileCorresponding = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                if (fileList[i])
                {
                    item = item + 1;
                    fileCorresponding[item - 1] = files[i];
                }
            }
            string[] SearchResult = new string[item];
            for (int i = 0; i < item; i++)
                SearchResult[i] = fileCorresponding[i];
            file = SearchResult;
            StartCoroutine(WaitAndPage(0.1F, 0));
        }
    }
    IEnumerator WaitAndPage(float time, int page)
    {
        yield return new WaitForSeconds(time);
        Page(page);
    }

    public void Share()
    {
        string filePath = file[SelectedLevel];

        string[] Name = filePath.Split(new string[] { "/", "\\" }, StringSplitOptions.None);
        string fileName = Name[Name.Length - 1].Replace(".level", "");
        ExtensionFilter[] extensionFilter = new ExtensionFilter[] { new ExtensionFilter("level file", "level") }; //Windows filter

#if UNITY_STANDALONE || UNITY_EDITOR
        NativeShare.SharePC(filePath, "", "", fileName, extensionFilter);
#elif UNITY_ANDORID || UNITY_IOS
        NativeShare.Share("try my new super level called "+ fileName + ", that I created on Angry Dash.", //body
        file[SelectedLevel], //path
        "", //url
        "Try my level on Angry Dash", //subject
        "text/plain", //mime
        true, //chooser
        "Select sharing app" //chooserText
        );
#endif
    }

    public void Publish()
    {
        if (!Editeur.CheckPublicID(file[SelectedLevel]))
        { Debug.LogWarning("Can't publish a level without Public ID or with a corrupted Public ID"); return; }

        UploadPanel.transform.GetChild(0).gameObject.SetActive(false);
        UploadPanel.transform.GetChild(1).gameObject.SetActive(false);
        UploadPanel.SetActive(true);

        WebClient client = new WebClient();
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string Result = "";

        string[] Name = file[SelectedLevel].Split(new string[] { "/", "\\" }, StringSplitOptions.None);
        string fileName = Name[Name.Length - 1].Replace(".level", "");

        string fileID = "";
        string[] f = File.ReadAllLines(file[SelectedLevel]);
        int pID = -1;
        for (int x = 0; x < f.Length; x++)
        {
            if (f[x].Contains("publicID = ") & pID == -1)
                pID = x;
        }
        if (pID != -1)
            fileID = f[pID].Replace("publicID = ", "");

        try { Result = client.DownloadString("https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/index.php?key=" + ConfigAPI.GetString("Account.Username") + "/" + fileName + "_" + fileID); } catch { UploadPanel.SetActive(false); return; }
        string[] keyFiles = Result.Split(new string[1] { "<BR />" }, StringSplitOptions.None);

        int length = keyFiles.Length;
        if (length > 0)
        {
            if (keyFiles[length - 1] == "")
                length = length - 1;
        }

        for (int i = 0; i < length; i++)
        {
            if (!string.IsNullOrEmpty(keyFiles[i]))
            {
                string[] keyFile = new string[1] { keyFiles[i] };
                if (keyFiles[i].Contains(" ; "))
                    keyFile = keyFiles[i].Split(new string[1] { " ; " }, System.StringSplitOptions.None);

                string level = "";
                string author = "";
                string description = "";
                float stars = 0;
                for (int l = 0; l < keyFile.Length; l++)
                {

                    string[] line = keyFile[l].Split(new string[1] { " = " }, System.StringSplitOptions.None);
                    if (line[0] == "level")
                        level = keyFile[l].Replace("level = ", "");
                    else if (line[0] == "author")
                        author = keyFile[l].Replace("author = ", "");
                    else if (line[0] == "description")
                        description = keyFile[l].Replace("description = ", "");
                }

                if (level == fileName & author == ConfigAPI.GetString("Account.Username"))
                {
                    Transform Uploaded = UploadPanel.transform.GetChild(1);
                    Uploaded.GetChild(1).GetChild(0).GetComponent<Text>().text = level;
                    Uploaded.GetChild(2).GetChild(0).GetComponent<Text>().text = description;
                    Uploaded.GetChild(3).GetComponent<Dropdown>().value = 0;
                    Uploaded.GetChild(3).GetComponent<Dropdown>().interactable = true;
                    Uploaded.GetChild(4).gameObject.SetActive(stars > 0);
                    for (int s = 0; s < 5; s++)
                    {
                        Transform go = Uploaded.GetChild(4).GetChild(s);
                        if (stars > s)
                            go.GetComponent<Image>().color = new Color32(255, 255, 0, 255);
                        else go.GetComponent<Image>().color = new Color32(180, 180, 180, 255);
                        if (stars > s + 0.5F)
                            go.GetChild(0).GetComponent<Image>().color = new Color32(255, 255, 0, 255);
                        else go.GetChild(0).GetComponent<Image>().color = new Color32(180, 180, 180, 255);
                    }
                    Uploaded.GetChild(5).GetComponent<Button>().interactable = false;
                    Uploaded.GetChild(5).GetComponent<Image>().color = new Color32(0, 255, 0, 255);

                    string result = "";
                    try { result = client.DownloadString("https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/files/" + ConfigAPI.GetString("Account.Username") + "/" + fileName + "_" + fileID + ".level"); } catch { UploadPanel.SetActive(false); return; }
                    if (result != File.ReadAllText(file[SelectedLevel]))
                    {
                        Uploaded.GetChild(6).gameObject.SetActive(true);
                        Uploaded.GetChild(5).GetComponent<Button>().interactable = true;
                        Uploaded.GetChild(5).GetComponent<Image>().color = new Color32(255, 255, 255, 255);
                    }
                    else Uploaded.GetChild(6).gameObject.SetActive(false);
                    Uploaded.gameObject.SetActive(true);
                    return;
                }
            }
        }

        Transform FirstTime = UploadPanel.transform.GetChild(0);
        FirstTime.GetChild(2).GetComponent<InputField>().text = fileName;
        FirstTime.GetChild(3).GetComponent<InputField>().text = Desc[SelectedLevel];
        FirstTime.gameObject.SetActive(true);
    }

    public void Publish_Upload(bool isNew)
    {
        if (!Editeur.CheckPublicID(file[SelectedLevel]))
            return;

        string lvlName = "";
        string lvlDesc = "";
        string lvlID = "";

        string[] f = File.ReadAllLines(file[SelectedLevel]);
        int pID = -1;
        for (int x = 0; x < f.Length; x++)
        {
            if (f[x].Contains("publicID = ") & pID == -1)
                pID = x;
        }
        if (pID != -1)
            lvlID = f[pID].Replace("publicID = ", "");

        if (isNew)
        {
            Transform FirstTime = UploadPanel.transform.GetChild(0);
            lvlName = FirstTime.GetChild(2).GetComponent<InputField>().text;
            lvlDesc = FirstTime.GetChild(3).GetComponent<InputField>().text;
            FirstTime.gameObject.SetActive(false);

            ChangName(lvlName);
            ChangDesc(lvlDesc);
        }
        else
        {
            if (SelectedLevel != -1)
            {
                string[] a = File.ReadAllLines(file[SelectedLevel]);

                int d = -1;
                for (int x = 0; x < a.Length; x++)
                {
                    if (a[x].Contains("description = ") & d == -1)
                        d = x;
                }
                if (d != -1)
                    lvlDesc = a[d].Replace("description = ", "");

                string[] Name = file[SelectedLevel].Split(new string[] { "/", "\\" }, StringSplitOptions.None);
                lvlName = Name[Name.Length - 1].Replace(".level", "");
            }
        }

        Transform Uploaded = UploadPanel.transform.GetChild(1);
        Uploaded.GetChild(1).GetChild(0).GetComponent<Text>().text = lvlName;
        Uploaded.GetChild(2).GetChild(0).GetComponent<Text>().text = lvlDesc;
        Uploaded.GetChild(3).GetComponent<Dropdown>().value = 0;
        Uploaded.GetChild(3).GetComponent<Dropdown>().interactable = false;
        Uploaded.GetChild(4).gameObject.SetActive(false);
        Uploaded.GetChild(5).GetComponent<Button>().interactable = false;
        Uploaded.GetChild(5).GetComponent<Image>().color = new Color32(180, 180, 180, 255);
        Uploaded.GetChild(6).gameObject.SetActive(false);
        Uploaded.gameObject.SetActive(true);

        WebClient client = new WebClient();
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

#if UNITY_EDITOR || UNITY_STANDALONE
        string accPath = Application.persistentDataPath.Replace("Angry Dash", "06Games Launcher/") + "account.account";
#elif UNITY_ANDROID
        string accPath = Application.persistentDataPath.Replace("AngryDash", "Launcher") + "/account.account";
#endif
        string[] details = File.ReadAllLines(accPath);
        string id = details[0].Replace("1 = ", "");
        string mdp = details[1].Replace("2 = ", "");
        string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/upload.php?id=" + id + "&mdp=" + mdp + "&level=" + lvlName + "&levelID=" + lvlID;
        byte[] responseArray = client.UploadData(URL, File.ReadAllBytes(file[SelectedLevel]));
        string Result = System.Text.Encoding.ASCII.GetString(responseArray);

        if (Result.Contains("Sucess"))
        {
            Uploaded.GetChild(5).GetComponent<Button>().interactable = false;
            Uploaded.GetChild(5).GetComponent<Image>().color = new Color32(0, 255, 0, 255);
        }
        else
        {
            Uploaded.GetChild(5).GetComponent<Button>().interactable = true;
            Uploaded.GetChild(5).GetComponent<Image>().color = new Color32(255, 0, 0, 255);
        }
    }
}
