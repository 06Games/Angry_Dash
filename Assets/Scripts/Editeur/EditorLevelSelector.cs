using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Tools;
using System.Net;

/// <summary> Script managing the user-created level selection menu </summary>
public class EditorLevelSelector : MonoBehaviour
{
    /// <summary> To change scene </summary>
    public LoadingScreenControl loadingScreenControl;
    /// <summary> How the levels should be sorted </summary>
    public enum SortMode
    {
        /// <summary> From newest to oldest </summary>
        Newest,
        /// <summary> From oldest to newest </summary>
        Oldest,
        /// <summary> From A to Z </summary>
        aToZ,
        /// <summary> From Z to A </summary>
        zToA
    }
    /// <summary> Current sort mode </summary>
    public SortMode sortMode;
    /// <summary> Search keywords </summary>
    public string keywords = "*";
    /// <summary> Where levels are saved, create the folder if it does not exist </summary>
    public string directory
    {
        get
        {
            string dir = Application.persistentDataPath + "/Levels/Edited Levels/";
            if (!Directory.Exists(dir)) //If the folder does not exist, then create it
                Directory.CreateDirectory(dir);
            return dir;
        }
    }
    /// <summary> Currently displayed files </summary>
    public FileInfo[] files;
    /// <summary> Index of the selected level, value equal to -1 if no level is selected </summary>
    public int currentFile = -1;
    /// <summary> The root of the selected level </summary>
    FileFormat.XML.RootElement xmlRoot;

    void Start()
    {
        //Initialization
        currentFile = -1; //No level is selected
        transform.GetChild(2).gameObject.SetActive(false); //Disables infos panel
        transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(0, 100); //Put the list in full screen

        //Displays levels
        Sort(SortMode.Newest);
    }

    /// <summary> Displays levels with a specified sort, should only be used in the editor </summary>
    /// <param name="sort">Sort type</param>
    public void Sort(int sort) { Sort((SortMode)sort); }
    /// <summary> Displays levels with a specified sort </summary>
    /// <param name="sort">Sort type</param>
    public void Sort(SortMode sort, bool reselect = true)
    {
        //Caches the level selected
        FileInfo selectedFile = null;
        if (files != null)
        {
            if (currentFile >= 0 & currentFile < files.Length)
                selectedFile = files[currentFile];
        }

        FileInfo[] FI = new DirectoryInfo(directory).GetFiles(keywords + ".level", SearchOption.AllDirectories); //Searches level containing the keywords

        //Sorts the files
        if (sort == SortMode.Newest) files = FI.OrderByDescending(f => f.LastWriteTime).ToArray();
        else if (sort == SortMode.Oldest) files = FI.OrderBy(f => f.LastWriteTime).ToArray();
        else if (sort == SortMode.aToZ) files = FI.OrderBy(f => f.Name).ToArray();
        else if (sort == SortMode.zToA) files = FI.OrderByDescending(f => f.Name).ToArray();
        sortMode = sort;

        //Disables the selected sorting button
        for (int i = 0; i < transform.GetChild(0).childCount - 2; i++)
            transform.GetChild(0).GetChild(i).GetComponent<Button>().interactable = (int)sort != i;

        //Removes the displayed levels
        Transform ListContent = transform.GetChild(1).GetChild(0).GetChild(0);
        for (int i = 1; i < ListContent.childCount; i++)
            Destroy(ListContent.GetChild(i).gameObject);

        //Deplays the levels
        ListContent.GetChild(0).gameObject.SetActive(false);
        int newSelect = -1;
        for (int i = 0; i < files.Length; i++)
        {
            Transform go = Instantiate(ListContent.GetChild(0).gameObject, ListContent).transform; //Creates a button
            int button = i;
            go.GetComponent<Button>().onClick.AddListener(() => Select(button)); //Sets the script to excute on click

            string fileName = PathExtensions.GetRelativePath(files[i].DirectoryName, directory).Replace("\\", "/");
            if (!string.IsNullOrEmpty(fileName)) fileName = fileName + "/";
            fileName = fileName + Path.GetFileNameWithoutExtension(files[i].Name);

            go.name = fileName; //Changes the editor gameObject name (useful only for debugging)
            go.GetChild(0).GetComponent<Text>().text = fileName; //Sets the level's name
            go.GetChild(1).GetComponent<Text>().text = files[i].LastWriteTime.Format(); //Sets the level's last open date
            go.gameObject.SetActive(true);
            if (selectedFile != null & reselect)
            {
                if (files[i].FullName == selectedFile.FullName) //Select the new button if it matches the old button selected
                    newSelect = i;
            }
        }
        if (newSelect >= 0 & reselect)
            StartCoroutine(ScrollAndSelectButton(newSelect));
    }

    /// <summary> Selects a level and scroll to it </summary>
    /// <param name="newSelect">Index of the level</param>
    public IEnumerator ScrollAndSelectButton(int newSelect)
    {
        Transform ListContent = transform.GetChild(1).GetChild(0).GetChild(0);
        yield return new WaitForEndOfFrame(); //Wait for the next frame
        Select(newSelect); //Selects the level
        ListContent.GetChild(newSelect + 1).GetComponent<Button>().interactable = false; //Disables the button
        transform.GetChild(1).GetComponent<ScrollRect>().SnapTo(ListContent.GetChild(newSelect + 1), this); //Scroll to the level
    }

    /// <summary> Changes search keywords </summary>
    /// <param name="input">Search bar</param>
    public void Filter(InputField input) { Filter(input.text); }
    /// <summary> Changes search keywords </summary>
    /// <param name="key">Search keywords</param>
    public void Filter(string key)
    {
        if (string.IsNullOrEmpty(key)) key = "*"; //If nothing is entered, display all levels
        else if (!key.Contains("*")) key = "*" + key + "*"; //If there is no specific filter, display all levels containing the keywords entered
        keywords = key;
        Sort(sortMode); //Refresh the list
    }

    /// <summary> Selects a level </summary>
    /// <param name="selected">Index of the level</param>
    public void Select(int selected)
    {
        Transform infos = transform.GetChild(2);
        try { xmlRoot = new FileFormat.XML.XML(File.ReadAllText(files[selected].FullName)).RootElement; } catch { xmlRoot = null; }

        //Description
        DescriptionEditMode(false);
        string Desc = LangueAPI.Get("native", "EditorEditInfosDescriptionError", "<color=red>Can not read the description</color>");
        if (xmlRoot != null) Desc = xmlRoot.GetItem("description").Value;
        infos.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = Desc.Format();

        //If infos panel is inactive, display it !
        if (!infos.gameObject.activeInHierarchy) StartCoroutine(InfosPanelAnimation(true));

        //Disables the selected level button
        Transform ListContent = transform.GetChild(1).GetChild(0).GetChild(0);
        for (int i = 1; i < ListContent.childCount; i++)
            ListContent.GetChild(i).GetComponent<Button>().interactable = (i - 1) != selected;

        currentFile = selected; //Set the level as selected
    }

    /// <summary> Starts the opening/closing animation of the info panel </summary>
    /// <param name="open">Opening animation ?</param>
    IEnumerator InfosPanelAnimation(bool open)
    {
        //Infos panel
        Transform infos = transform.GetChild(2);
        Vector2 pos = new Vector2(infos.position.x, 137.5F);
        if (open) infos.position = new Vector2(pos.x, pos.y * -1);
        else infos.position = pos;
        infos.gameObject.SetActive(true);

        //List
        Vector2 ListOffset = new Vector2(0, 275) - new Vector2(0, 100);
        if (open) transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(0, 100);
        else transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(0, 275);

        int Speed = 15; //Number of frames to make the animation
        Vector2 newpos = infos.position; //Position of the info panel
        Vector2 newListOffset = transform.GetChild(1).GetComponent<RectTransform>().offsetMin; //Position of the list
        for (int i = 0; i < Speed; i++)
        {
            //Infos panel
            if (open) newpos.y = newpos.y + (pos.y * 2 / Speed);
            else newpos.y = newpos.y - (pos.y * 2 / Speed);
            infos.position = newpos;

            //List
            if (open) newListOffset.y = newListOffset.y + (ListOffset.y / Speed);
            else newListOffset.y = newListOffset.y - (ListOffset.y / Speed);
            transform.GetChild(1).GetComponent<RectTransform>().offsetMin = newListOffset;

            yield return new WaitForEndOfFrame(); //Wait for the next frame
        }
        infos.gameObject.SetActive(open);
    }

    /// <summary> Create a new level </summary>
    public void CreateNewLevel(Transform CreatePanel)
    {
        string name = CreatePanel.GetChild(1).GetComponent<InputField>().text;
        if (!string.IsNullOrEmpty(name))
        {
            string path = directory + name + ".level";
            if (!File.Exists(path))
            {
                string desc = CreatePanel.GetChild(2).GetComponent<InputField>().text;
                loadingScreenControl.LoadScreen("Editor", new string[] { "Home/Editor/Editor", "Create", path, desc.Unformat() });
            }
            else CreatePanel.GetChild(1).GetChild(5).gameObject.SetActive(true);
        }
        else CreatePanel.GetChild(1).GetChild(5).gameObject.SetActive(true);
    }

    public void SwitchDescriptionEditMode() { DescriptionEditMode(transform.GetChild(2).GetChild(0).GetChild(1).gameObject.activeInHierarchy); }
    public void DescriptionEditMode(bool edit)
    {
        Transform desc = transform.GetChild(2).GetChild(0);
        string description = "";
        if (edit) description = desc.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text;
        else description = desc.GetChild(2).GetComponent<InputField>().text;

        desc.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = description;
        desc.GetChild(1).gameObject.SetActive(!edit);
        desc.GetChild(2).GetComponent<InputField>().text = description;
        desc.GetChild(2).gameObject.SetActive(edit);
        if (edit) desc.GetChild(2).GetComponent<Selectable>().Select();
    }

    public void DescriptionEdit(InputField field) { DescriptionEdit(field.text); }
    public void DescriptionEdit(string description)
    {
        if (xmlRoot != null)
        {
            xmlRoot.GetItem("description").Value = description;
            File.WriteAllText(files[currentFile].FullName, xmlRoot.xmlFile.ToString());
        }
    }

    /// <summary> Play the selected level </summary>
    public void PlayCurrentLevel() { PlayLevel(currentFile); }
    /// <summary> Play the level at the index specified </summary>
    /// <param name="index">Index of desired level</param>
    public void PlayLevel(int index)
    {
        History.LvlPlayed(files[index].FullName, "P");
        loadingScreenControl.LoadScreen("Player",
            new string[] { "Home/Editor/Editor", "File", files[index].FullName });
    }

    /// <summary> Edit the selected level </summary>
    public void EditCurrentLevel() { EditLevel(currentFile); }
    /// <summary> Edit the level at the index specified </summary>
    /// <param name="index">Index of desired level</param>
    public void EditLevel(int index) { loadingScreenControl.LoadScreen("Editor", new string[] { "Home/Editor/Editor", "Edit", files[index].FullName }); }

    /// <summary> Copy the selected level </summary>
    public void CopyCurrentLevel() { CopyLevel(currentFile); }
    /// <summary> Copy the level at the index specified </summary>
    /// <param name="index">Index of desired level</param>
    public void CopyLevel(int index)
    {
        File.Copy(files[index].FullName, //Copy the level
            files[index].DirectoryName + "/" + Path.GetFileNameWithoutExtension(files[index].FullName) + " - Copy.level");
        Sort(sortMode, false); //Refresh the list
        StartCoroutine(ScrollAndSelectButton(index)); //Scroll to the copied level
    }

    /// <summary> Delete the selected level </summary>
    public void DeleteCurrentLevel() { DeleteLevel(currentFile); }
    /// <summary> Delete the level at the index specified </summary>
    /// <param name="index">Index of desired level</param>
    public void DeleteLevel(int index)
    {
        File.Delete(files[index].FullName); //Delete the level
        Sort(sortMode); //Refresh the list
        StartCoroutine(InfosPanelAnimation(false)); //Remove the infos panel
    }

    /// <summary> Share the selected level </summary>
    public void ShareCurrentLevel() { ShareLevel(currentFile); }
    /// <summary> Share the level at the index specified </summary>
    /// <param name="index">Index of desired level</param>
    public void ShareLevel(int index)
    {
#if UNITY_STANDALONE || UNITY_EDITOR
        NativeShare.SharePC(files[index].FullName, "", "", //Path
            Path.GetFileNameWithoutExtension(files[index].Name), //Default file name
            new Crosstales.FB.ExtensionFilter[] { new Crosstales.FB.ExtensionFilter("level file", "level") } //Windows filter
        );
#elif UNITY_ANDROID || UNITY_IOS
        NativeShare.Share(
            "try my new super level called " + Path.GetFileNameWithoutExtension(files[index].Name) + ", that I created on Angry Dash.", //body
            files[index].FullName, //path
            "", //url
            "Try my level on Angry Dash", //subject
            "text/plain", //mime
            true, //chooser
            "Select sharing app" //chooserText
        );
#endif
    }

    /// <summary> Publish the selected level </summary>
    public void PublishCurrentLevel() { PublishLevel(currentFile); }
    /// <summary> Publish the level at the index specified </summary>
    /// <param name="index">Index of desired level</param>
    public void PublishLevel(int index)
    {
        string id = Account.Username;
        string mdp = Account.Password;

        MenuManager PublishPanel = transform.GetChild(5).GetComponent<MenuManager>();
        PublishPanel.Array(0);
        PublishPanel.gameObject.SetActive(true);

        WebClient client = new WebClient();
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/upload.php?id=" + id + "&mdp=" + mdp + "&level=" + Path.GetFileNameWithoutExtension(files[index].Name);

        string lvl = File.ReadAllText(files[index].FullName); //Read the level file
        if (FileFormat.XML.Utils.IsValid(lvl)) // If the level is in XML, minimize it
            lvl = lvl.Replace("\n", "").Replace("\r", "").Replace("\t", "");
        client.UploadDataCompleted += (sender, e) =>
        {
            string Result = "";
            if (e.Error != null) Result = e.Error.Message;
            else Result = System.Text.Encoding.ASCII.GetString(e.Result);

            if (Result.Contains("Success")) PublishPanel.Array(2);
            else
            {
                PublishPanel.Array(1);
                PublishPanel.GO[1].transform.GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "EditorEditPublishError", "<color=red>Error</color> : [0]", Result);
            }
        };
        client.UploadDataAsync(new System.Uri(URL.Replace(" ", "%20")), lvl.ToByte());

    }
}
