using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DownloadedMusicsManager : MonoBehaviour
{
    ScrollRect scroll;
    void Start()
    {
        scroll = GetComponent<ScrollRect>();
        Refresh();
    }

    void Refresh()
    {
        for (int i = 1; i < scroll.content.childCount; i++)
            Destroy(scroll.content.GetChild(i).gameObject);

        GameObject Template = scroll.content.GetChild(0).gameObject;
        FileInfo[] files = new DirectoryInfo(Application.persistentDataPath + "/Musics/").GetFiles("* - *", SearchOption.AllDirectories);
        string mime = "application/ogg";

        for (int i = 0; i < files.Length; i++)
        {
            TagLib.Tag TL = TagLib.File.Create(files[i].FullName, mime, TagLib.ReadStyle.None).Tag;
            Transform go = Instantiate(Template, scroll.content).transform;

            go.GetChild(0).GetComponent<Text>().text = LangueAPI.StringWithArgument("native", "", new string[] { TL.Title, TL.Performers[0] }, "[0]\n<color=grey>by [1]</color>");
            int button = i;
            go.GetChild(1).GetChild(0).GetComponent<Button>().onClick.AddListener(() => Play(files[button].FullName));
            go.GetChild(1).GetChild(1).GetComponent<Button>().onClick.AddListener(() => Delete(files[button].FullName));
            go.gameObject.SetActive(true);
        }
    }

    public void Delete(string path) { File.Delete(path); Refresh(); }

    float defaultMusicPos = 0;
    string curentlyPlaying = "";
    public void Play(string path)
    {
        menuMusic mm = null;
        if (GameObject.Find("Audio") != null) mm = GameObject.Find("Audio").GetComponent<menuMusic>();
        else return;

        if (mm.GetComponent<AudioSource>().clip == SoundAPI.Sound.Get("native/main") | curentlyPlaying != path) //Play
        {
            defaultMusicPos = mm.GetComponent<AudioSource>().time;
            mm.LoadUnpackagedMusic(path);
            curentlyPlaying = path;
        }
        else //Stop
            mm.StartDefault(defaultMusicPos);
    }
}
