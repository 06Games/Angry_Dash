using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class ProfileLevels : MonoBehaviour
{
    /// <summary> URL of the community server root </summary>
    public readonly string serverURL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/";
    /// <summary> The path where the levels are saved localy </summary>
    public string savePath { get; private set; }
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

    string[] items;

    void Start()
    {
        savePath = Application.persistentDataPath + "/Levels/Edited Levels/";
        //Displays levels
        Sort(SortMode.aToZ);
    }

    /// <summary> Refresh levels list </summary>
    public void Refresh() { Sort(sortMode); }

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
        string Result = client.DownloadString(serverURL + "index.php?key=" + Account.Username + "/"); //Searches user's levels
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
        items = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            string[] file = new string[1] { files[i] };
            if (files[i].Contains(" ; "))
                file = files[i].Split(new string[1] { " ; " }, System.StringSplitOptions.None);

            bool Continue = true;
            for (int l = 0; l < file.Length & Continue; l++)
            {
                string[] line = file[l].Split(new string[1] { " = " }, System.StringSplitOptions.None);
                if (line[0] == "level")
                {
                    items[i] = file[l].Replace("level = ", "");
                    Continue = false;
                }
            }
        }

        //Deplays the levels
        ListContent.GetChild(0).gameObject.SetActive(false);
        for (int i = 0; i < items.Length; i++)
        {
            Transform go = Instantiate(ListContent.GetChild(0).gameObject, ListContent).transform; //Creates an item
            go.name = items[i]; //Changes the editor gameObject name (useful only for debugging)

            int button = i;
            go.GetChild(0).GetComponent<Text>().text = items[i]; //Sets the level's name
            go.GetChild(1).GetComponent<Button>().onClick.AddListener(() => {
                Transform panel = transform.GetChild(2);
                panel.GetChild(0).GetComponent<Text>().text = LangueAPI.StringWithArgument("native", "ProfileMyLevelsDeleteWarning", items[button], "Are you sure you want to delete [0] ?");
                panel.GetChild(1).GetComponent<Button>().onClick.AddListener(() => { Delete(button); panel.gameObject.SetActive(false); });
                panel.gameObject.SetActive(true);
            });
            if (File.Exists(savePath + "/" + items[i] + ".level")) go.GetChild(2).GetComponent<Button>().interactable = false;
            else go.GetChild(2).GetComponent<Button>().onClick.AddListener(() => Download(button));
            go.GetChild(3).GetComponent<Button>().onClick.AddListener(() => Collaborate(button));
            go.gameObject.SetActive(true);
        }
    }

    public void Download(int i)
    {
        if (i >= items.Length) return;
        string url = serverURL + "files/" + Account.Username + "/" + items[i] + ".level";
        string path = savePath + items[i] + ".level";
        WebClient client = new WebClient();
        client.Encoding = System.Text.Encoding.UTF8;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        string file = "";
        try { file = client.DownloadString(url.Replace(" ", "%20")); } catch { }
        if (file != "")
        {
            File.WriteAllText(path, file);
        }
        else if (Application.isEditor) Debug.LogWarning("Failed to download :\n" + url);
    }

    public void Delete(int i)
    {
        if (i >= items.Length) return;
        string url = serverURL+"delete.php?id=" + Account.Username + "&mdp=" + Account.Password + "&level=" + items[i];
        WebClient client = new WebClient();
        client.Encoding = System.Text.Encoding.UTF8;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

        string file = "";
        try { file = client.DownloadString(url.Replace(" ", "%20")); } catch { }
        if (file.Contains("Success")) { Sort(sortMode); }
    }

    public void Collaborate(int i)
    {
        if (i >= items.Length) return;
        Debug.LogWarning("Currently, collaborations aren't available");
    }
}
