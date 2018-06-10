using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class EditorSelect : MonoBehaviour
{
    public GameObject Selector;
    public GameObject Info;
    public GameObject Cam;
    public GameObject _NewG;
    InputField[] _New = new InputField[2];
    public Editeur editeur;

    public int SelectedLevel = -1;
    public string[] Desc;
    public string[] file;
    int lastItem = -1;

    private void Start()
    {
        _New[0] = _NewG.transform.GetChild(0).GetChild(2).gameObject.GetComponent<InputField>();
        _New[1] = _NewG.transform.GetChild(0).GetChild(3).gameObject.GetComponent<InputField>();
        NewStart();
    }

    public void NewStart()
    {
        lastItem = -1;
        SelectedLevel = -1;
        _NewG.SetActive(false);

        Cam.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, -10);
        Cam.GetComponent<Camera>().orthographicSize = Screen.height / 2;

        string directory = Application.persistentDataPath + "/Saved Level/";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        file = Directory.GetFiles(directory);
        int Files = file.Length;
        Desc = new string[Files];

        Page(1);
    }

    public static string FormatedDate(DateTime DT)
    {
        //string a = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern.Replace("/", "'/'");
        string a = "dd'/'MM'/'yyyy";
        return DT.ToString(a);
    }

    public void ChangLevel(int button)
    {
        SelectedLevel = button+(lastItem-4);

        if (button != -1)
            Info.transform.GetChild(1).GetChild(0).gameObject.GetComponent<InputField>().text = Desc[button];
        else Info.transform.GetChild(1).GetChild(0).gameObject.GetComponent<InputField>().text = "";
    }

    public void ChangDesc(InputField IF)
    {
        if (SelectedLevel != -1)
        {
            Desc[SelectedLevel] = IF.text;
            string[] a = File.ReadAllLines(file[SelectedLevel]);
            a[1] = Desc[SelectedLevel];
            File.WriteAllLines(file[SelectedLevel], a);
        }
    }

    public void Play()
    {
        if(GameObject.Find("Audio") != null)
            GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
        editeur.EditFile(file[SelectedLevel]);
    }

    public void Copy()
    {
        File.Copy(file[SelectedLevel], file[SelectedLevel].Replace(".level", " - Copy.level"));
        NewStart();
    }

    public void Del()
    {
        File.Delete(file[SelectedLevel]);
        NewStart();
        ChangLevel(-1);
    }

    public void New()
    {
        bool n = File.Exists(Application.persistentDataPath + "/Saved Level/" + _New[0].text.ToLower() + ".level");
        bool d = _New[1].text == "" | _New[1].text == null;

        if (!n & !d)
        {
            editeur.CreateFile(_New[0].text.ToLower(), Application.persistentDataPath + "/Saved Level/", _New[1].text);

            if (GameObject.Find("Audio") != null)
                GameObject.Find("Audio").GetComponent<menuMusic>().Stop();
        }
        else
        {
            CheckNewLevelName(_New[0]);
            CheckNewLevelDesc(_New[1]);
        }
    }
    public void CheckNewLevelName(InputField IF)
    {
        Image i = IF.transform.GetChild(3).gameObject.GetComponent<Image>();
        if (File.Exists(Application.persistentDataPath + "/Saved Level/" + IF.text.ToLower() + ".level") | IF.text == "")
            i.color = new Color32(163, 0, 0, 255);
        else i.color = new Color32(129, 129, 129, 255);
    }
    public void CheckNewLevelDesc(InputField IF)
    {
        Image i = IF.transform.GetChild(3).gameObject.GetComponent<Image>();
        if (IF.text == "" | IF.text == null)
            i.color = new Color32(163, 0, 0, 255);
        else i.color = new Color32(129, 129, 129, 255);
    }

    public void Page(int v)
    {
       int f = lastItem + 1;

        if(v == -1)
            f = lastItem - 9;

        lastItem = f + 4;

        for (int i = 0; i < 5; i++)
        {
            Transform go = Selector.transform.GetChild(i + 1);

            if (file.Length > f)
            {
                go.gameObject.SetActive(true);

                string[] Name = file[f].Split(new string[] { "/", "\\" }, StringSplitOptions.None);
                DateTime UTC = File.GetLastWriteTime(file[i]);

                go.GetChild(0).GetComponent<Text>().text = Name[Name.Length - 1].Replace(".level", "");
                go.GetChild(1).GetComponent<Text>().text = FormatedDate(UTC);
                Desc[i] = File.ReadAllLines(file[f])[1];

                f = f + 1;
            }
            else go.gameObject.SetActive(false);
        }

        Selector.transform.GetChild(6).GetComponent<Button>().interactable = lastItem < file.Length - 1;
        Selector.transform.GetChild(0).GetComponent<Button>().interactable = lastItem - 5 > -1;
    }
}
