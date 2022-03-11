using System;
using System.IO;
using AngryDash.Language;
using TagLib;
using UnityEngine;
using UnityEngine.UI;
using File = TagLib.File;

public class DownloadedMusicsManager : MonoBehaviour
{
    private ScrollRect scroll;

    private void Start()
    {
        scroll = GetComponent<ScrollRect>();
        Refresh();
    }

    private void Refresh()
    {
        for (var i = 1; i < scroll.content.childCount; i++)
            Destroy(scroll.content.GetChild(i).gameObject);

        var Template = scroll.content.GetChild(0).gameObject;
        var files = new DirectoryInfo(Application.persistentDataPath + "/Musics/").GetFiles("* - *", SearchOption.AllDirectories);
        var mime = "application/ogg";

        for (var i = 0; i < files.Length; i++)
        {
            try
            {
                var TL = File.Create(files[i].FullName, mime, ReadStyle.None).Tag;
                var go = Instantiate(Template, scroll.content).transform;

                go.GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "SettingsSoundDownloadedItem", "[0]\n<color=grey>by [1]</color>", TL.Title, TL.Performers[0]);
                var button = i;
                go.GetChild(1).GetChild(0).GetComponent<Button>().onClick.AddListener(() => Play(files[button].FullName));
                go.GetChild(1).GetChild(1).GetComponent<Button>().onClick.AddListener(() => Delete(files[button].FullName));
                go.gameObject.SetActive(true);
            }
            catch (Exception e) { Logging.Log(e); }
        }
    }

    public void Delete(string path) { System.IO.File.Delete(path); Refresh(); }

    private float defaultMusicPos;
    private string curentlyPlaying = "";
    public void Play(string path)
    {
        if (GameObject.Find("Audio") == null) return;

        var mm = GameObject.Find("Audio").GetComponent<MenuMusic>();
        if (mm.PlayingMainMusic | curentlyPlaying != path) //Play
        {
            defaultMusicPos = mm.GetComponent<AudioSource>().time;
            mm.LoadUnpackagedMusic(path);
            curentlyPlaying = path;
        }
        else mm.StartDefault(defaultMusicPos); //Stop
    }

    private void OnDisable()
    {
        if (GameObject.Find("Audio") == null) return;

        var mm = GameObject.Find("Audio").GetComponent<MenuMusic>();
        if (!mm.PlayingMainMusic) mm.StartDefault(defaultMusicPos);
    }
}
