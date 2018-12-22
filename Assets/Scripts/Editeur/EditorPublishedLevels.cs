using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelItem
{
    public string Name = "";
    public string Author = "";
    public string Description = "";
    public string Music = "";
    public string[] Data = new string[0];
    public LevelItem() { }
    public LevelItem(string name, string author = "", string[] data = null, string description = "", string music = "")
    {
        Name = name;
        Author = author;
        if (data == null) Data = new string[0];
        else Data = data;
        Description = description;
        Music = music;
    }


    private static readonly System.Xml.Serialization.XmlSerializer _serializer = new System.Xml.Serialization.XmlSerializer(typeof(LevelItem));
    public override string ToString()
    {
        var settings = new System.Xml.XmlWriterSettings
        {
            NewLineHandling = System.Xml.NewLineHandling.Entitize
        };

        using (var stream = new StringWriter())
        using (var writer = System.Xml.XmlWriter.Create(stream, settings))
        {
            _serializer.Serialize(writer, this);

            return stream.ToString();
        }
    }
    public static LevelItem Parse(string data)
    {
        if (string.IsNullOrEmpty(data))
            return null;

        using (var stream = new StringReader(data))
        using (var reader = System.Xml.XmlReader.Create(stream))
        {
            return (LevelItem)_serializer.Deserialize(reader);
        }
    }
}

public class EditorPublishedLevels : MonoBehaviour
{

    /// <summary> To change scene </summary>
    public LoadingScreenControl loadingScreenControl;
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
        WebClient client = new WebClient();
        client.Encoding = System.Text.Encoding.UTF8;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string Result = client.DownloadString(serverURL + "index.php?key=" + keywords); //Searches level containing the keywords
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
            go.GetChild(1).GetComponent<Text>().text = LangueAPI.StringWithArgument("native", "CHANGETHISLATER", items[i].Author, "by [0]"); //Sets the level's author
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
        infos.GetChild(0).GetChild(1).GetChild(1).GetComponent<Text>().text = LangueAPI.StringWithArgument("native", "CHANGETHISLATER", items[selected].Author, "by [0]");
        infos.GetChild(1).GetChild(1).GetComponent<ScrollRect>().content.GetChild(0).GetComponent<Text>().text = items[selected].Description;
        if (!string.IsNullOrEmpty(items[selected].Music))
        {
            string[] music = items[selected].Music.Split(new string[] { " - " }, System.StringSplitOptions.None);
            if (music.Length == 2)
            {
                infos.GetChild(3).GetChild(1).GetComponent<Text>().text = music[1] + "\n<color=grey>by " + music[0] + "</color>";
                infos.GetChild(3).GetChild(2).gameObject.SetActive(!File.Exists(Application.persistentDataPath + "/Musics/" + items[selected].Music));
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
        item.Data = client.DownloadString(url).Split(new string[] { "\n" }, System.StringSplitOptions.None);
        loadingScreenControl.LoadScreen("Player", new string[] { "Home/Editor/Community Levels", "Data", item.ToString() });
    }

    
    /// <summary> Download the selected level's  </summary>
    public void DownloadCurrentLevelMusic() { DownloadLevelMusic(currentFile); }
    /// <summary> Play the music of level at the index specified </summary>
    /// <param name="index">Index of desired level</param>
    public void DownloadLevelMusic(int index)
    {
        if (InternetAPI.IsConnected())
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Musics/"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Musics/");

            string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/musics/mp3/" +
                Soundboard.WithoutSpecialCharacters(items[index].Music).Replace(" ", "%20") + ".mp3";

            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileCompleted += wc_DownloadFileCompleted;

                string path = Application.persistentDataPath + "/Musics/";
                if (Soundboard.NativeFileFormat() == AudioType.OGGVORBIS)
                    path = Application.temporaryCachePath + "/";
                wc.DownloadFileAsync(new System.Uri(URL), path + Soundboard.WithoutSpecialCharacters(items[index].Music), index);

                transform.GetChild(2).GetChild(3).GetChild(2).gameObject.SetActive(false);
                transform.GetChild(2).GetChild(3).GetChild(3).gameObject.SetActive(true);
            }
        }
    }
    private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) { }
    private void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        if (e.Cancelled) { print("The download has been cancelled"); return; }
        if (e.Error != null) { Debug.LogError("An error ocurred while trying to download file\n" + e.Error); return; }
        int index = (int)e.UserState;

        if (Soundboard.NativeFileFormat() == AudioType.OGGVORBIS)
        {
            string fileName = Soundboard.WithoutSpecialCharacters(items[index].Music);
            UnityThread.executeInUpdate(() =>
            {
                if (File.Exists(Application.temporaryCachePath + "/" + fileName + ".mp3"))
                    File.Delete(Application.temporaryCachePath + "/" + fileName + ".mp3");
                File.Move(Application.temporaryCachePath + "/" + fileName, Application.temporaryCachePath + "/" + fileName + ".mp3");

                if (File.Exists(Application.persistentDataPath + "/Musics/" + fileName + ".ogg"))
                    File.Delete(Application.persistentDataPath + "/Musics/" + fileName + ".ogg");


                FFmpeg.FFmpegAPI.Convert(Application.temporaryCachePath + "/" + fileName + ".mp3", Application.temporaryCachePath + "/" + fileName + ".ogg", new FFmpeg.handler(ConvertEnd, index));
            });
        }
        else transform.GetChild(2).GetChild(3).GetChild(3).gameObject.SetActive(false);
    }
    void ConvertEnd(object sender, Tools.BetterEventArgs args)
    {
        int index = (int)args.UserState;
        UnityThread.executeInUpdate(() =>
        {
            string fileName = Soundboard.WithoutSpecialCharacters(items[index].Music);
            if (File.Exists(Application.temporaryCachePath + "/" + fileName + ".mp3"))
                File.Delete(Application.temporaryCachePath + "/" + fileName + ".mp3");
            if (File.Exists(Application.persistentDataPath + "/Musics/" + fileName))
                File.Delete(Application.persistentDataPath + "/Musics/" + fileName);
            File.Move(Application.temporaryCachePath + "/" + fileName + ".ogg", Application.persistentDataPath + "/Musics/" + items[index].Music);

            transform.GetChild(2).GetChild(3).GetChild(3).gameObject.SetActive(false);
        });
    }
}
