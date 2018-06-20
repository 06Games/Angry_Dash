using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class EditorSearch : MonoBehaviour
{

    public Transform rZone;
    string[] files;

    void Start()
    {
        /*files = Directory.GetFiles(Application.persistentDataPath + "/Saved Level/");*/
        Search(null);
    }

    public void Search(InputField IF)
    {
        files = new string[0];
        bool[] fileList = new bool[files.Length];

        if (IF != null)
        {
            if (!string.IsNullOrEmpty(IF.text))
            {
                string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/levels/community/index.php?key=" + IF.text;
                WebClient client = new WebClient();
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                string Result = client.DownloadString(URL.Replace(" ", "%20"));
                files = Result.Split(new string[1] { "<BR />" }, StringSplitOptions.None);
                
                fileList = new bool[files.Length];
                for (int i = 0; i < files.Length; i++)
                {
                    string[] file = new string[1] { files[i] };
                    if (files[i].Contains(" ; "))
                        file = files[i].Split(new string[1] { " ; " }, System.StringSplitOptions.None);

                    string keyWord = IF.text;
                    if (IF.text.Contains("/"))
                        keyWord = IF.text.Split(new string[1] { "/" }, System.StringSplitOptions.None)[1];
                    if (file.Length > 0)
                        fileList[i] = file[0].Contains(keyWord);
                    else fileList[i] = false;
                }
            }
        }

        int item = 0;
        string[] level = new string[files.Length];
        string[] author = new string[files.Length];
        string[] id = new string[files.Length];

        for (int i = 0; i < files.Length; i++)
        {
            if (fileList[i])
            {
                string[] file = new string[1] { files[i] };
                if (files[i].Contains(" ; "))
                    file = files[i].Split(new string[1] { " ; " }, System.StringSplitOptions.None);

                if (file.Length > 0) { level[item] = file[0]; }
                if (file.Length > 1) { author[item] = file[1]; } else author[item] = "Somebody";
                if (file.Length > 2) { id[item] = file[2]; } else id[item] = "Unkown";

                item = item + 1;
            }
        }
        for (int i = 0; i < 4; i++)
        {
            Transform go = rZone.GetChild(i);
            if (i < item)
            {
                go.gameObject.SetActive(true);
                go.GetChild(0).GetComponent<Text>().text = level[i];
                go.GetChild(1).GetComponent<Text>().text = "by " + author[i];
                go.GetChild(2).GetComponent<Text>().text = "ID : " + "ABCDEF";
            }
            else go.gameObject.SetActive(false);
        }
    }
}
