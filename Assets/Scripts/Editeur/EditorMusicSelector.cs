using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using Level;
using Tools;
using System.Collections;

public class EditorMusicSelector : MonoBehaviour
{
    /// <summary> The editor script </summary>
    public Editeur Editor;
    /// <summary> URL of the music server root </summary>
    public readonly string serverURL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/musics/";
    /// <summary> How the musics should be sorted </summary>
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
    public string keywords = "*";
    /// <summary> Index of the selected music, value equal to -1 if no music is selected </summary>
    public int currentFile = -1;
    /// <summary> Songs received from the server </summary>
    public SongItem[] items;

    void Start()
    {
        //Initialization
        transform.GetChild(2).gameObject.SetActive(false);

        //Displays levels
        Sort(SortMode.aToZ);
    }

    /// <summary> Displays musics with a specified sort, should only be used in the editor </summary>
    /// <param name="sort">Sort type</param>
    public void Sort(int sort) { Sort((SortMode)sort); }
    /// <summary> Displays musics with a specified sort </summary>
    /// <param name="sort">Sort type</param>
    public void Sort(SortMode sort, bool reselect = true)
    {
        WebClient client = new WebClient();
        client.Encoding = System.Text.Encoding.UTF8;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string Result = client.DownloadString(serverURL + "index.php?key=" + keywords); //Searches music containing the keywords
        string[] files = new string[0];
        if (!string.IsNullOrEmpty(Result)) files = Result.Split(new string[1] { "<BR />" }, System.StringSplitOptions.RemoveEmptyEntries);

        //Sorts the files
        if (sort == SortMode.aToZ) files = files.OrderBy(f => f.ToString()).ToArray();
        else if (sort == SortMode.zToA) files = files.OrderByDescending(f => f.ToString()).ToArray();
        sortMode = sort;

        //Disables the selected sorting button
        for (int i = 0; i < transform.GetChild(0).childCount - 1; i++)
            transform.GetChild(0).GetChild(i).GetComponent<Button>().interactable = (int)sort != i;

        //Removes the displayed musics
        Transform ListContent = transform.GetChild(1).GetChild(0).GetChild(0);
        for (int i = 1; i < ListContent.childCount; i++) Destroy(ListContent.GetChild(i).gameObject);

        //Get Infos
        items = new SongItem[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            string[] file = files[i].Split(new string[1] { " ; " }, System.StringSplitOptions.None);
            if (file.Length == 3)
            {
                items[i] = new SongItem()
                {
                    URL = file[0],
                    Artist = file[1].HtmlDecode(),
                    Name = file[2].HtmlDecode()
                };
            }
            else items[i] = new SongItem();
        }
        if (items.Length == 0)
        {
            string[] sFiles = System.IO.Directory.GetFiles(Application.persistentDataPath + "/Musics/");
            items = new SongItem[sFiles.Length];
            for (int i = 0; i < sFiles.Length; i++)
            {
                TagLib.Tag TL = TagLib.File.Create(sFiles[i], "application/ogg", TagLib.ReadStyle.None).Tag;
                items[i] = new SongItem() { URL = sFiles[i], Artist = TL.Performers[0], Name = TL.Title, Licence = "" };
            }
        }

        //Deplays the musics
        ListContent.GetChild(0).gameObject.SetActive(false);
        for (int i = 0; i < items.Length; i++)
        {
            Transform go = Instantiate(ListContent.GetChild(0).gameObject, ListContent).transform; //Creates a button
            int button = i;
            go.GetComponent<Button>().onClick.AddListener(() => Select(button)); //Sets the script to excute on click
            go.name = items[i].Name; //Changes the editor gameObject name (useful only for debugging)

            go.GetChild(0).GetComponent<Text>().text = items[i].Name; //Sets the music's name
            go.GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "editor.options.music.artist", "<color=grey>by [0]</color>", items[i].Artist); //Sets the music's artist
            go.GetChild(2).gameObject.SetActive(Editor.level.music == items[i]);
            go.gameObject.SetActive(true);
        }
    }

    /// <summary> Changes search keywords </summary>
    /// <param name="input">Search bar</param>
    public void Filter(InputField input) { Filter(input.text); }
    /// <summary> Changes search keywords </summary>
    /// <param name="key">Search keywords</param>
    public void Filter(string key)
    {
        if (string.IsNullOrEmpty(key)) key = "*"; //If nothing is entered, display all musics
        keywords = key;
        Sort(sortMode); //Refresh the list
    }

    /// <summary> Selects a music </summary>
    /// <param name="selected">Index of the music</param>
    public void Select(int selected)
    {
        Transform infos = transform.GetChild(2);
        infos.GetChild(0).GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "editor.options.music.details.infos", "[0]\n<color=grey><size=50>by [1]</size></color>", items[selected].Name, items[selected].Artist);

        WebClient c = new WebClient();
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string URL = serverURL + "licences/" + items[selected].Artist + " - " + items[selected].Name + ".txt";
        URL = URL.Replace(" ", "%20");
        try { items[selected].Licence = c.DownloadString(URL); }
        catch (System.Exception e) { Debug.LogWarning(e.Message); items[selected].Licence = "Error"; }
        infos.GetChild(1).GetComponent<Text>().text = items[selected].Licence;

        infos.GetChild(2).GetChild(0).gameObject.SetActive(
            !System.IO.File.Exists(Application.persistentDataPath + "/Musics/" + items[selected].Artist + " - " + items[selected].Name)
        );
        infos.GetChild(2).GetChild(1).GetChild(1).GetComponent<Slider>().value = 0;
        infos.GetChild(2).GetChild(1).GetChild(1).GetChild(0).GetComponent<Scrollbar>().size = 0;
        infos.GetChild(2).GetChild(2).gameObject.SetActive(Editor.level.music != items[selected]);

        infos.gameObject.SetActive(true);
        currentFile = selected; //Set the music as selected
    }

    SoundAPI.Load load = null;
    public void PlayMusic()
    {
        GameObject go = GameObject.Find("Audio");
        if (go != null)
        {
            if (!go.GetComponent<AudioSource>().isPlaying)
            {
                string fileName = items[currentFile].Artist + " - " + items[currentFile].Name;
                if (System.IO.File.Exists(Application.persistentDataPath + "/Musics/" + fileName))
                    load = new SoundAPI.Load(Application.persistentDataPath + "/Musics/" + fileName);
                else load = new SoundAPI.Load(new System.Uri(serverURL + "files/" + fileName + ".ogg"));

                Slider state = transform.GetChild(2).GetChild(2).GetChild(1).GetChild(1).GetComponent<Slider>();
                load.Readable += (sender, e) =>
                {
                    float timepos = 0;
                    AudioSource source = go.GetComponent<AudioSource>();
                    if (((AudioClip)e.UserState).name == source.clip.name) timepos = source.time;
                    go.GetComponent<menuMusic>().LoadMusic((AudioClip)e.UserState, timepos);
                    StartCoroutine(PlayMusicState(source, state));
                };
                load.ReadProgressChanged += (sender, e) =>
                     state.transform.GetChild(0).GetComponent<Scrollbar>().size = (float)e.UserState;
                StartCoroutine(load.Start(false));
            }
            else go.GetComponent<menuMusic>().Pause();
        }
    }
    public void StopMusic()
    {
        if (load != null) { load.Cancel(); load = null; }
        GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
    }
    IEnumerator PlayMusicState(AudioSource audio, Slider state)
    {
        float timepos = state.value;
        yield return new WaitForEndOfFrame();
        if (state.value == timepos) state.value = audio.time / audio.clip.length;
        else audio.time = timepos * audio.clip.length;

        if (!audio.isPlaying) yield return new WaitForSeconds(0.1F); //Let the music start...
        if (audio.isPlaying) StartCoroutine(PlayMusicState(audio, state));
    }

    public void Choose()
    {
        Editor.level.music = items[currentFile];
        transform.GetChild(2).GetChild(2).GetChild(2).gameObject.SetActive(false);
    }

    public void Download()
    {
        string filename = Application.persistentDataPath + "/Musics/" + items[currentFile].Artist + " - " + items[currentFile].Name;
        string url = serverURL + "files/" + items[currentFile].Artist + " - " + items[currentFile].Name + ".ogg";
        WebClient client = new WebClient();
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        Transform downBtn = transform.GetChild(2).GetChild(2).GetChild(0);
        client.DownloadProgressChanged += (sender, e) => downBtn.GetChild(0).GetComponent<Scrollbar>().size = e.ProgressPercentage / 100F;
        client.DownloadFileCompleted += (sender, e) => downBtn.gameObject.SetActive(false);
        client.DownloadFileAsync(new System.Uri(url), filename);
    }
}
