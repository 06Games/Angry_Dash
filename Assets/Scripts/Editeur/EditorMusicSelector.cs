using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using AngryDash.Language;
using Level;
using SoundAPI;
using TagLib;
using Tools;
using UnityEngine;
using UnityEngine.UI;
using File = TagLib.File;

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

    private void Start()
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
    public void Sort(SortMode sort)
    {
        if (InternetAPI.IsConnected())
        {
            var client = new WebClient();
            client.Encoding = Encoding.UTF8;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            client.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null) { Logging.Log(e.Error); return; }
                var Result = e.Result;

                var files = new string[0];
                if (!string.IsNullOrEmpty(Result)) files = Result.Split(new string[1] { "<BR />" }, StringSplitOptions.RemoveEmptyEntries);

                //Sorts the files
                if (sort == SortMode.aToZ) files = files.OrderBy(f => f.ToString()).ToArray();
                else if (sort == SortMode.zToA) files = files.OrderByDescending(f => f.ToString()).ToArray();
                sortMode = sort;

                //Disables the selected sorting button
                for (var i = 0; i < transform.GetChild(0).childCount - 1; i++)
                    transform.GetChild(0).GetChild(i).GetComponent<Button>().interactable = (int)sort != i;

                //Removes the displayed musics
                var ListContent = transform.GetChild(1).GetChild(0).GetChild(0);
                for (var i = 1; i < ListContent.childCount; i++) Destroy(ListContent.GetChild(i).gameObject);

                //Get Infos
                items = new SongItem[files.Length];
                for (var i = 0; i < files.Length; i++)
                {
                    var file = files[i].Split(new string[1] { " ; " }, StringSplitOptions.None);
                    if (file.Length == 3)
                    {
                        items[i] = new SongItem
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
                    var sFiles = Directory.GetFiles(Application.persistentDataPath + "/Musics/");
                    items = new SongItem[sFiles.Length];
                    for (var i = 0; i < sFiles.Length; i++)
                    {
                        var TL = File.Create(sFiles[i], "application/ogg", ReadStyle.None).Tag;
                        items[i] = new SongItem { URL = sFiles[i], Artist = TL.Performers[0], Name = TL.Title, Licence = "" };
                    }
                }

                //Deplays the musics
                ListContent.GetChild(0).gameObject.SetActive(false);
                for (var i = 0; i < items.Length; i++)
                {
                    var go = Instantiate(ListContent.GetChild(0).gameObject, ListContent).transform; //Creates a button
                    var button = i;
                    go.GetComponent<Button>().onClick.AddListener(() => Select(button)); //Sets the script to excute on click
                    go.name = items[i].Name; //Changes the editor gameObject name (useful only for debugging)

                    go.GetChild(0).GetComponent<Text>().text = items[i].Name; //Sets the music's name
                    go.GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "editor.options.music.artist", "<color=grey>by [0]</color>", items[i].Artist); //Sets the music's artist
                    go.GetChild(2).gameObject.SetActive(Editor.level.music == items[i]);
                    go.gameObject.SetActive(true);
                }
            };
            client.DownloadStringAsync(new Uri(serverURL + "index.php?key=" + keywords)); //Searches music containing the keywords
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
        var infos = transform.GetChild(2);
        infos.GetChild(0).GetChild(1).GetComponent<Text>().text = LangueAPI.Get("native", "editor.options.music.details.infos", "[0]\n<color=grey><size=50>by [1]</size></color>", items[selected].Name, items[selected].Artist);

        var c = new WebClient();
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        var URL = serverURL + "licences/" + items[selected].Artist + " - " + items[selected].Name + ".txt";
        URL = URL.RemoveSpecialCharacters().Replace(" ", "%20");
        try { items[selected].Licence = c.DownloadString(URL); }
        catch (Exception e)
        {
            Debug.LogWarning("Error when loading: " + URL + "\n" + e.Message);
            items[selected].Licence = "Error";
        }
        infos.GetChild(1).GetComponent<Text>().text = items[selected].Licence;

        infos.GetChild(2).GetChild(0).gameObject.SetActive(
            !System.IO.File.Exists(Application.persistentDataPath + "/Musics/" + items[selected].Artist + " - " + items[selected].Name)
        );
        infos.GetChild(2).GetChild(1).GetChild(0).GetComponent<Button>().interactable = true;
        infos.GetChild(2).GetChild(1).GetChild(1).GetComponent<Slider>().value = 0;
        infos.GetChild(2).GetChild(1).GetChild(1).GetChild(0).GetComponent<Scrollbar>().size = 0;
        infos.GetChild(2).GetChild(2).gameObject.SetActive(Editor.level.music != items[selected]);

        infos.gameObject.SetActive(true);
        currentFile = selected; //Set the music as selected
    }

    private Load load;
    public void PlayMusic()
    {
        var playPanel = transform.GetChild(2).GetChild(2).GetChild(1);
        var go = GameObject.Find("Audio");
        if (go != null)
        {
            if (!go.GetComponent<AudioSource>().isPlaying)
            {
                var fileName = items[currentFile].Artist + " - " + items[currentFile].Name;
                Load newLoad = null;
                if (System.IO.File.Exists(Application.persistentDataPath + "/Musics/" + fileName))
                    newLoad = new Load(Application.persistentDataPath + "/Musics/" + fileName);
                else newLoad = new Load(new Uri((serverURL + "files/" + fileName + ".ogg").RemoveSpecialCharacters()));

                var source = go.GetComponent<AudioSource>();
                var state = playPanel.GetChild(1).GetComponent<Slider>();
                if (load == newLoad)
                {
                    go.GetComponent<MenuMusic>().Play();
                    StartCoroutine(PlayMusicState(source, state));
                }
                else
                {
                    load = newLoad;
                    load.Readable += clip =>
                    {
                        playPanel.GetChild(0).GetComponent<Button>().interactable = true;
                        float timepos = 0;
                        if (clip.name == source.clip.name) timepos = source.time;
                        go.GetComponent<MenuMusic>().LoadMusic(clip, timepos);
                        StartCoroutine(PlayMusicState(source, state));
                    };
                    load.ReadProgressChanged += progress =>
                         state.transform.GetChild(0).GetComponent<Scrollbar>().size = progress;
                    playPanel.GetChild(0).GetComponent<Button>().interactable = false;
                    StartCoroutine(load.Start(false));
                }
            }
            else go.GetComponent<MenuMusic>().Pause();
        }
    }
    public void StopMusic()
    {
        if (load != null) { load.Cancel(); load = null; }
        GameObject.Find("Audio").GetComponent<MenuMusic>().Stop();
    }

    private IEnumerator PlayMusicState(AudioSource audio, Slider state)
    {
        var timepos = state.value;
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
        var filename = Application.persistentDataPath + "/Musics/" + items[currentFile].Artist + " - " + items[currentFile].Name;
        if (!Directory.Exists(Application.persistentDataPath + "/Musics/"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Musics/");
        var URL = serverURL + "files/" + items[currentFile].Artist + " - " + items[currentFile].Name + ".ogg";
        var url = new Uri(URL.RemoveSpecialCharacters().Replace(" ", "%20"));
        var client = new WebClient();
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        var downBtn = transform.GetChild(2).GetChild(2).GetChild(0);
        client.DownloadProgressChanged += (sender, e) =>
            downBtn.GetChild(0).GetComponent<Scrollbar>().size = e.ProgressPercentage / 100F;
        client.DownloadDataCompleted += (sender, e) =>
        {
            if (!e.Cancelled)
            {
                System.IO.File.WriteAllBytes(filename, e.Result);
                downBtn.gameObject.SetActive(false);
            }
            else Logging.Log(e.Error, LogType.Error);
        };
        Logging.Log("Start downloading music from '" + url.AbsoluteUri + "'");
        client.DownloadDataAsync(url);
    }
}
