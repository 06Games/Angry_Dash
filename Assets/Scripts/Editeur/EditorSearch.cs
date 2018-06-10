using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class EditorSearch : MonoBehaviour
{

    public Transform rZone;
    string[] files;

    void Start()
    {
        files = Directory.GetFiles(Application.persistentDataPath + "/Saved Level/");
        Search(null);
    }

    public void Search(InputField IF)
    {
        bool[] fileList = new bool[files.Length];
        if (IF != null)
        {
            if (!string.IsNullOrEmpty(IF.text))
            {
                for (int i = 0; i < files.Length; i++)
                {
                    string[] file = files[i].Split(new string[1] { "/" }, System.StringSplitOptions.None);
                    string fileName = file[file.Length - 1].Replace(".level", "");
                    fileList[i] = fileName.Contains(IF.text);
                }
            }
        }

        int item = 0;
        string[] fileCorresponding = new string[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            if (fileList[i])
            {
                item = item + 1;
                fileCorresponding[item-1] = files[i];
            }
        }
        for (int i = 0; i < 4; i++)
        {
            Transform go = rZone.GetChild(i);
            if (i < item)
            {
                go.gameObject.SetActive(true);
                string[] file = fileCorresponding[i].Split(new string[1] { "/" }, System.StringSplitOptions.None);
                go.GetChild(0).GetComponent<Text>().text = file[file.Length - 1].Replace(".level", "");
                go.GetChild(1).GetComponent<Text>().text = "by " + "Somebody";
                go.GetChild(2).GetComponent<Text>().text = "ID : " + "ABCDEF";
            }
            else go.gameObject.SetActive(false);
        }
    }
}
