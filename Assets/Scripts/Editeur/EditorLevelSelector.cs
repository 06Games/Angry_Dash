﻿using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using _06Games.Account;
using AngryDash.Image.Reader;
using AngryDash.Language;
using FileFormat;
using FileFormat.XML;
using SFB;
using Tools;
using UnityEngine;
using UnityEngine.UI;

/// <summary> Script managing the user-created level selection menu </summary>
public class EditorLevelSelector : MonoBehaviour
{
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
            var dir = Application.persistentDataPath + "/Levels/Edited Levels/";
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
    private RootElement xmlRoot;

    private void Start()
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

        var FI = new DirectoryInfo(directory).GetFiles(keywords + ".level", SearchOption.AllDirectories); //Searches level containing the keywords

        //Sorts the files
        if (sort == SortMode.Newest) files = FI.OrderByDescending(f => f.LastWriteTime).ToArray();
        else if (sort == SortMode.Oldest) files = FI.OrderBy(f => f.LastWriteTime).ToArray();
        else if (sort == SortMode.aToZ) files = FI.OrderBy(f => f.Name).ToArray();
        else if (sort == SortMode.zToA) files = FI.OrderByDescending(f => f.Name).ToArray();
        sortMode = sort;

        //Disables the selected sorting button
        for (var i = 0; i < transform.GetChild(0).childCount - 2; i++)
            transform.GetChild(0).GetChild(i).GetComponent<Button>().interactable = (int)sort != i;

        //Removes the displayed levels
        var ListContent = transform.GetChild(1).GetChild(0).GetChild(0);
        for (var i = 1; i < ListContent.childCount; i++)
            Destroy(ListContent.GetChild(i).gameObject);

        //Deplays the levels
        ListContent.GetChild(0).gameObject.SetActive(false);
        var newSelect = -1;
        for (var i = 0; i < files.Length; i++)
        {
            var go = Instantiate(ListContent.GetChild(0).gameObject, ListContent).transform; //Creates a button
            var button = i;
            go.GetComponent<Button>().onClick.AddListener(() => Select(button)); //Sets the script to excute on click

            var fileName = PathExtensions.GetRelativePath(files[i].DirectoryName, directory).Replace("\\", "/");
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
        var ListContent = transform.GetChild(1).GetChild(0).GetChild(0);
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
        if (currentFile == selected) //Close the panel
        {
            var infos = transform.GetChild(2).gameObject;
            if (infos.activeInHierarchy)
            {
                StartCoroutine(InfosPanelAnimation(false));
                transform.GetChild(1).GetChild(0).GetChild(0).GetChild(currentFile + 1).GetComponent<UImage_Reader>().autoChange = true;
                currentFile = -1;
            }
        }
        else
        {
            var infos = transform.GetChild(2);
            try { xmlRoot = new XML(File.ReadAllText(files[selected].FullName)).RootElement; } catch { xmlRoot = null; }

            //Description
            DescriptionEditMode(false);
            var Desc = LangueAPI.Get("native", "EditorEditInfosDescriptionError", "<color=red>Can not read the description</color>");
            if (xmlRoot != null) Desc = xmlRoot.GetItem("description").Value;
            infos.GetChild(0).GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = Desc.Format();

            //If infos panel is inactive, display it !
            if (!infos.gameObject.activeInHierarchy) StartCoroutine(InfosPanelAnimation(true));

            //Disables the selected level button
            var ListContent = transform.GetChild(1).GetChild(0).GetChild(0);
            for (var i = 1; i < ListContent.childCount; i++)
            {
                //ListContent.GetChild(i).GetComponent<Button>().interactable = (i - 1) != selected;
                var reader = ListContent.GetChild(i).GetComponent<UImage_Reader>();
                reader.autoChange = (i - 1) != selected;
                reader.StartAnimating((i - 1) == selected ? 3 : 0);
            }

            currentFile = selected; //Set the level as selected
        }
    }

    /// <summary> Starts the opening/closing animation of the info panel </summary>
    /// <param name="open">Opening animation ?</param>
    private IEnumerator InfosPanelAnimation(bool open)
    {
        //Infos panel
        var infos = (RectTransform)transform.GetChild(2);
        var pos = new Vector2(0, 137.5F);
        if (open) infos.position = new Vector2(pos.x, pos.y * -1);
        else infos.position = pos;
        infos.gameObject.SetActive(true);

        //List
        var ListOffset = new Vector2(0, 275) - new Vector2(0, 100);
        if (open) transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(0, 100);
        else transform.GetChild(1).GetComponent<RectTransform>().offsetMin = new Vector2(0, 275);

        var Speed = 15; //Number of frames to make the animation
        Vector2 newpos = infos.position; //Position of the info panel
        var newListOffset = transform.GetChild(1).GetComponent<RectTransform>().offsetMin; //Position of the list
        for (var i = 0; i < Speed; i++)
        {
            //Infos panel
            if (open) newpos.y = newpos.y + (pos.y * 2 / Speed);
            else newpos.y = newpos.y - (pos.y * 2 / Speed);
            infos.anchoredPosition = newpos;

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
        var name = CreatePanel.GetChild(1).GetComponent<InputField>().text;
        if (!string.IsNullOrEmpty(name))
        {
            var path = directory + name + ".level";
            if (!File.Exists(path))
            {
                var desc = CreatePanel.GetChild(2).GetComponent<InputField>().text;
                SceneManager.LoadScene("Editor", new[] { "Home/Editor/Editor", "Create", path, desc.Unformat() });
            }
            else CreatePanel.GetChild(1).GetChild(5).gameObject.SetActive(true);
        }
        else CreatePanel.GetChild(1).GetChild(5).gameObject.SetActive(true);
    }

    public void SwitchDescriptionEditMode() { DescriptionEditMode(transform.GetChild(2).GetChild(0).GetChild(1).gameObject.activeInHierarchy); }
    public void DescriptionEditMode(bool edit)
    {
        var desc = transform.GetChild(2).GetChild(0);
        var description = "";
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
        SceneManager.LoadScene("Player", new[] { "Home/Editor/Editor", "File", files[index].FullName });
    }

    /// <summary> Edit the selected level </summary>
    public void EditCurrentLevel() { EditLevel(currentFile); }
    /// <summary> Edit the level at the index specified </summary>
    /// <param name="index">Index of desired level</param>
    public void EditLevel(int index) { SceneManager.LoadScene("Editor", new[] { "Home/Editor/Editor", "Edit", files[index].FullName }); }

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
            new[] { new ExtensionFilter("level file", "level") } //Windows filter
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
        var PublishPanel = transform.GetChild(5).GetComponent<MenuManager>();
        PublishPanel.Array(0);
        PublishPanel.gameObject.SetActive(true);

        API.CheckAccountFile((success, msg) =>
        {
            if (success)
            {
                var client = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                var URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/upload.php?token=" + API.Information.token + "&level=" + Path.GetFileNameWithoutExtension(files[index].Name);

                var lvl = File.ReadAllText(files[index].FullName); //Read the level file
                if (Utils.IsValid(lvl)) // If the level is in XML, minimize it
                    lvl = lvl.Replace("\n", "").Replace("\r", "").Replace("\t", "");
                client.UploadDataCompleted += (sender, e) =>
                {
                    if (e.Error != null) Error(e.Error.Message);
                    else
                    {
                        var response = new JSON(Encoding.UTF8.GetString(e.Result));
                        if (response.Value<string>("state") == "Done") PublishPanel.Array(2);
                        else Error(response.Value<string>("state"));
                    }
                };
                client.UploadDataAsync(new Uri(URL.Replace(" ", "%20")), lvl.ToByte());

            }
            else Error(msg);
            void Error(string error)
            {
                PublishPanel.Array(1);
                PublishPanel.GO[1].transform.GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "EditorEditPublishError", "<color=red>Error</color> : [0]", error);

            }
        });
    }
}
