using _06Games.Account;
using AngryDash.Image.Reader;
using AngryDash.Language;
using Level;
using System.IO;
using System.Linq;
using System.Net;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class EditorPublishedLevels : MonoBehaviour
{
    /// <summary> URL of the community server root </summary>
    public readonly string serverURL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/";
    /// <summary> How the levels should be sorted </summary>
    public enum SortMode
    {
        /// <summary> From A to Z </summary>
        aToZ,
        /// <summary> From Z to A </summary>
        zToA
    }
    /// <summary> Current sort mode </summary>
    public SortMode sortMode;
    /// <summary> Search keywords </summary>
    public string keywords = "/";
    /// <summary> Index of the selected level, value equal to -1 if no level is selected </summary>
    public int currentFile = -1;

    FileFormat.XML.RootElement markXML;
    int userIndex = -1;

    public LevelItem[] items;
    void Start()
    {
        //Initialization
        transform.GetChild(2).gameObject.SetActive(false);

        //Displays levels
        Sort(SortMode.aToZ);
    }

    /// <summary> Displays levels with a specified sort, should only be used in the editor </summary>
    /// <param name="sort">Sort type</param>
    public void Sort(int sort) { Sort((SortMode)sort); }
    /// <summary> Displays levels with a specified sort </summary>
    /// <param name="sort">Sort type</param>
    public void Sort(SortMode sort, bool reselect = true)
    {
        if (InternetAPI.IsConnected())
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            client.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null) { Logging.Log(e.Error); return; }
                string Result = e.Result;

                string[] files = new string[0];
                if (!string.IsNullOrEmpty(Result)) files = Result.Split(new string[1] { "<BR />" }, System.StringSplitOptions.None);

                //Sorts the files
                if (sort == SortMode.aToZ) files = files.OrderBy(f => f.ToString()).ToArray();
                else if (sort == SortMode.zToA) files = files.OrderByDescending(f => f.ToString()).ToArray();
                sortMode = sort;

                //Disables the selected sorting button
                for (int i = 0; i < transform.GetChild(0).childCount - 1; i++)
                    transform.GetChild(0).GetChild(i).GetComponent<Button>().interactable = (int)sort != i;

                //Removes the displayed levels
                Transform ListContent = transform.GetChild(1).GetChild(0).GetChild(0);
                for (int i = 1; i < ListContent.childCount; i++)
                    Destroy(ListContent.GetChild(i).gameObject);

                //Get Infos
                items = new LevelItem[files.Length];
                for (int i = 0; i < files.Length; i++)
                {
                    string[] file = new string[1] { files[i] };
                    if (files[i].Contains(" ; "))
                        file = files[i].Split(new string[1] { " ; " }, System.StringSplitOptions.None);

                    items[i] = new LevelItem();
                    for (int l = 0; l < file.Length; l++)
                    {
                        string[] line = file[l].Split(new string[1] { " = " }, System.StringSplitOptions.None);
                        if (line[0] == "level")
                            items[i].Name = file[l].Replace("level = ", "");
                        else if (line[0] == "author")
                            items[i].Author = file[l].Replace("author = ", "");
                        else if (line[0] == "description")
                            items[i].Description = file[l].Replace("description = ", "");
                        else if (line[0] == "music")
                            items[i].Music = file[l].Replace("music = ", "");
                    }
                }

                //Deplays the levels
                ListContent.GetChild(0).gameObject.SetActive(false);
                for (int i = 0; i < items.Length; i++)
                {
                    Transform go = Instantiate(ListContent.GetChild(0).gameObject, ListContent).transform; //Creates a button
                    int button = i;
                    go.GetComponent<Button>().onClick.AddListener(() => Select(button)); //Sets the script to excute on click
                    go.name = items[i].Name; //Changes the editor gameObject name (useful only for debugging)

                    go.GetChild(0).GetComponent<Text>().text = items[i].Name; //Sets the level's name
                    go.GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "EditorCommunityLevelsAuthor", "by [0]", items[i].Author); //Sets the level's author
                    go.gameObject.SetActive(true);
                }
            };
            client.DownloadStringAsync(new System.Uri(serverURL + "index.php?key=" + keywords)); //Searches level containing the keywords
        }
    }

    /// <summary> Changes search keywords </summary>
    /// <param name="input">Search bar</param>
    public void Filter(InputField input) { Filter(input.text); }
    /// <summary> Changes search keywords </summary>
    /// <param name="key">Search keywords</param>
    public void Filter(string key)
    {
        if (string.IsNullOrEmpty(key)) key = "/"; //If nothing is entered, display all levels
        keywords = key;
        Sort(sortMode); //Refresh the list
    }

    /// <summary> Selects a level </summary>
    /// <param name="selected">Index of the level</param>
    public void Select(int selected)
    {
        Transform infos = transform.GetChild(2);
        infos.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>().text = items[selected].Name;
        infos.GetChild(0).GetChild(1).GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "EditorCommunityLevelsAuthor", "by [0]", items[selected].Author);
        infos.GetChild(1).GetChild(1).GetComponent<ScrollRect>().content.GetChild(0).GetComponent<Text>().text = items[selected].Description;

        WebClient client = new WebClient();
        client.Encoding = System.Text.Encoding.UTF8;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string URL = serverURL + "mark.php?action=get&level=" + items[selected].Author + "/" + items[selected].Name;
        markXML = new FileFormat.XML.XML(client.DownloadString(URL)).RootElement;

        float mark = -1;
        userIndex = -1;
        Transform comments = infos.GetChild(2).GetChild(1).GetComponent<ScrollRect>().content;
        for (int i = 1; i < comments.childCount; i++) Destroy(comments.GetChild(i).gameObject);
        if (markXML.GetItems("item") != null)
        {
            int coef = 0;
            foreach (FileFormat.XML.Item item in markXML.GetItems("item"))
            {
                string user = item.GetItem("user").Value;
                System.DateTime.TryParse(item.GetItem("date").Value, out System.DateTime date);
                string markV = item.GetItem("mark").Value;
                string comment = item.GetItem("comment").Value;

                if (user == API.Information.username) userIndex = coef;
                if (float.TryParse(markV, out float itemMark))
                {
                    mark = (mark * coef + itemMark) / (coef + 1);
                    coef++;
                }

                if (!string.IsNullOrEmpty(comment))
                {
                    Transform cGO = Instantiate(comments.GetChild(0).gameObject, comments).transform;
                    cGO.GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "EditorCommunityLevelsInfosCommentHeader", "[0] <i><color=grey>[1]</color></i>", user, date.ToString(System.Threading.Thread.CurrentThread.CurrentUICulture));
                    cGO.GetChild(1).GetComponent<Text>().text = comment.HtmlDecode();
                    cGO.gameObject.SetActive(true);
                }
            }
        }

        for (int s = 0; s < 2; s++)
        {
            float note = mark;
            if (s == 0 & userIndex >= 0) float.TryParse(markXML.GetItems("item")[userIndex].GetItem("mark").Value, out note);

            Transform stars = infos.GetChild(2).GetChild(0).GetChild(s).GetChild(1);
            foreach (Transform go in stars) Destroy(go.gameObject);
            for (int i = 0; i < 10 & !(note == -1 & s == 1); i++)
            {
                GameObject go = new GameObject(((i * 0.5F) + 0.5F).ToString());
                go.transform.parent = stars;
                if (i / 2F != i / 2) go.AddComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
                else go.AddComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                if (s == 0)
                {
                    int button = i;
                    go.AddComponent<Button>().onClick.AddListener(() => Mark(button * 0.5F));
                }

                string id = "native/GUI/editorMenu/communityLevels/starUnassigned";
                try { if (i < note * 2) id = "native/GUI/editorMenu/communityLevels/starAssigned"; } catch { }
                go.AddComponent<Image>();
                go.AddComponent<UImage_Reader>().SetID(id).LoadAsync();
            }
            stars.parent.GetChild(2).gameObject.SetActive(note != -1);
            stars.parent.GetChild(2).GetComponent<Text>().text = LangueAPI.Get("native", "EditorCommunityLevelsInfosStarsCount", "[0]/5", note.ToString("0.#"));
        }

        if (!string.IsNullOrEmpty(items[selected].Music))
        {
            string[] music = items[selected].Music.Split(new string[] { " - " }, System.StringSplitOptions.None);
            if (music.Length == 2)
            {
                infos.GetChild(3).GetChild(1).GetComponent<Text>().text = music[1] + "\n" + LangueAPI.Get("native", "EditorCommunityLevelsAuthor", "<color=grey>by [0]</color>", music[0]);
                infos.GetChild(3).GetChild(2).gameObject.SetActive(!File.Exists(Application.persistentDataPath + "/Musics/" + items[selected].Music));
                infos.GetChild(3).GetChild(3).gameObject.SetActive(false);
                infos.GetChild(3).gameObject.SetActive(true);
            }
            else infos.GetChild(3).gameObject.SetActive(false);
        }
        else infos.GetChild(3).gameObject.SetActive(false);

        infos.gameObject.SetActive(true);
        currentFile = selected; //Set the level as selected
    }

    /// <summary> Play the selected level </summary>
    public void PlayCurrentLevel() { PlayLevel(currentFile); }
    /// <summary> Play the level at the index specified </summary>
    /// <param name="index">Index of desired level</param>
    public void PlayLevel(int index)
    {
        string url = serverURL + "files/" + items[index].Author + "/" + items[index].Name + ".level";
        WebClient client = new WebClient();
        client.Encoding = System.Text.Encoding.UTF8;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        LevelItem item = items[index];
        item.Data = client.DownloadString(url).Replace("\r", "");
        SceneManager.LoadScene("Player", new string[] { "Home/Editor/Community Levels", "Data", item.ToString() });
    }


    /// <summary> Download the selected level's  </summary>
    public void DownloadCurrentLevelMusic() { DownloadLevelMusic(currentFile); }
    /// <summary> Play the music of level at the index specified </summary>
    /// <param name="index">Index of desired level</param>
    public void DownloadLevelMusic(int index)
    {
        if (InternetAPI.IsConnected())
        {
            string path = Application.persistentDataPath + "/Musics/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/musics/files/" +
                items[index].Music.HtmlDecode().Replace(" ", "%20") + ".ogg";

            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += wc_DownloadFileCompleted;

                wc.DownloadFileAsync(new System.Uri(URL), path + items[index].Music.HtmlDecode(), index);

                transform.GetChild(2).GetChild(3).GetChild(2).gameObject.SetActive(false);
                transform.GetChild(2).GetChild(3).GetChild(3).gameObject.SetActive(true);
            }
        }
    }
    private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        var unit = new string[] { "B", "KB", "MB", "GB", "TB" };
        double downloadedSize = e.BytesReceived;
        double totalSize = e.TotalBytesToReceive;
        int pourcentage = e.ProgressPercentage;

        //Downloaded size
        int sizePower = 0;
        if (totalSize > 0) sizePower = DependenciesManager.GetCorrectUnit(totalSize);
        else sizePower = DependenciesManager.GetCorrectUnit(downloadedSize);
        totalSize = System.Math.Round(totalSize / Mathf.Pow(1000, sizePower), 1);
        downloadedSize = System.Math.Round(downloadedSize / Mathf.Pow(1000, sizePower), 1);

        UnityThread.executeInUpdate(() =>
        {
            string downloaded = LangueAPI.Get("native", $"download.state.{unit[sizePower]}", $"[0] {unit[sizePower]} out of [1] {unit[sizePower]}", downloadedSize.ToString(), totalSize > 0 ? totalSize.ToString() : "~");
            string pourcent = LangueAPI.Get("native", "download.state.percentage", "[0]%", pourcentage.ToString("00"));

            Text DownloadInfo = transform.GetChild(2).GetChild(3).GetChild(3).GetComponent<Text>();
            DownloadInfo.gameObject.SetActive(true);
            DownloadInfo.text = downloaded + " - <color=grey>" + pourcent + "</color>";
        });
    }
    private void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        if (e.Cancelled) { print("The download has been cancelled"); return; }
        if (e.Error != null) { Debug.LogError("An error ocurred while trying to download file\n" + e.Error); return; }
        int index = (int)e.UserState;

        UnityThread.executeInUpdate(() => transform.GetChild(2).GetChild(3).GetChild(3).gameObject.SetActive(false));
    }

    public void Mark(float note)
    {
        WebClient client = new WebClient();
        client.Encoding = System.Text.Encoding.UTF8;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        API.CheckAccountFile((success, msg) =>
        {
            if (success)
            {
                string URL = serverURL + "mark.php?action=set&token=" + API.Information.token + "&level=" + items[currentFile].Author + "/" + items[currentFile].Name + "&mark=" + (note + 0.5F);
                string result = client.DownloadString(URL);
                if (result.Contains("Success")) Select(currentFile);
                else Debug.LogError("Connection error: " + result);
            }
            else Debug.LogError(msg);
        });
    }

    public void NewComment(Transform panel)
    {
        string commentText = "";
        if (userIndex >= 0) commentText = markXML.GetItems("item")[userIndex].GetItem("comment").Value.HtmlDecode();

        panel.GetChild(0).GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "EditorCommunityLevelsInfosCommentWrite", "Write a Comment\n<size=50><color=grey>about [0]</color></size>", items[currentFile].Name);
        panel.GetChild(1).GetChild(0).GetChild(1).GetComponent<InputField>().text = commentText;
        panel.gameObject.SetActive(true);
    }

    public void SubmitComment(Transform panel)
    {
        WebClient client = new WebClient();
        client.Encoding = System.Text.Encoding.UTF8;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        API.CheckAccountFile((success, msg) =>
        {
            if (success)
            {
                string URL = serverURL + "mark.php?action=set&token=" + API.Information.token + "&level=" + items[currentFile].Author + "/" + items[currentFile].Name
                    + "&comment=" + panel.GetChild(1).GetChild(0).GetChild(1).GetComponent<InputField>().text.HtmlEncode();
                string result = client.DownloadString(URL);
                if (result.Contains("Success"))
                {
                    Select(currentFile);
                    panel.gameObject.SetActive(false);
                }
                else Debug.LogError("Connection error: " + result.Replace("<BR />", "\n"));
            }
            else Debug.LogError(msg);
        });
    }
}
