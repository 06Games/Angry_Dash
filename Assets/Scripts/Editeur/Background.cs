﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Tools;

public class Background : MonoBehaviour {

    public int Selected;
    public Editeur Editor;
    string file;
    Sprite_API.Sprite_API_Data[] sp;
    Sprite_API.JSON_PARSE_DATA[] jsonData;

    public ColorPicker CP;

    private void Start()
    {
        if (string.IsNullOrEmpty(Editor.file)) gameObject.SetActive(false);
    }

    public void Charg()
    {
        if (sp == null)
        {
            int bgNumber = 12;

            sp = new Sprite_API.Sprite_API_Data[bgNumber];
            jsonData = new Sprite_API.JSON_PARSE_DATA[bgNumber];
            for (int i = 0; i < bgNumber; i++)
            {
                string baseID = "native/BACKGROUNDS/" + i;

                FileFormat.JSON.JSON json = new FileFormat.JSON.JSON("");
                if (File.Exists(Sprite_API.Sprite_API.spritesPath(baseID + ".json")))
                    json = new FileFormat.JSON.JSON(File.ReadAllText(Sprite_API.Sprite_API.spritesPath(baseID + ".json")));
                jsonData[i] = Sprite_API.Sprite_API.Parse(baseID, json);
                

                sp[i] = Sprite_API.Sprite_API.GetSprites(jsonData[i].path[0], jsonData[i].border[0]);
            }
        }
    }

    void Update () {
        if (file != Editor.file)
        {
            int d = -1;
            for (int x = 0; x < Editor.component.Length; x++)
            {
                if (Editor.component[x].Contains("background = ") & d == -1)
                {
                    d = x;
                    x = Editor.component.Length;
                }
            }

            if (d != -1)
            {
                Selected = int.Parse(Editor.component[d].Replace("background = ", "").Split(new string[1] { "; " }, System.StringSplitOptions.None)[0]);
                CP.CurrentColor = Editeur.HexToColor(Editor.component[d].Replace("background = ", "").Split(new string[1] { "; " }, System.StringSplitOptions.None)[1]);
                Page(0);
            }
        }

        file = Editor.file;
        if (Editor.file != "")
        {
            Color32 color = CP.CurrentColor;
            for (int i = 0; i < transform.GetChild(0).childCount-2; i++)
            {
                Transform trans = transform.GetChild(0).GetChild(i+1);
                trans.GetChild(0).GetComponent<Image>().color = color;

                if (i+j == Selected)
                    trans.GetComponent<Image>().color = new Color32(210, 210, 210, 255);
                else trans.GetComponent<Image>().color = new Color32(92, 92, 92, 255);
            }

            int d = -1;
            for (int x = 0; x < Editor.component.Length; x++)
            {
                if (Editor.component[x].Contains("background = ") & d == -1)
                {
                    d = x;
                    x = Editor.component.Length;
                }
            }
            if(d != -1)
                Editor.component[d] = "background = " + Selected.ToString("0") + "; " + Editeur.ColorToHex(color);

            ActualiseFond(Editor);
        }
	}

    public void ChangFond(int i) { Selected = i+j; }

    public void ActualiseFond(Editeur Editor)
    {
        if (sp == null) Charg();
        Transform go = GameObject.Find("BackgroundDiv").transform;

        int d = -1;
        for (int x = 0; x < Editor.component.Length; x++)
        {
            if (Editor.component[x].Contains("background = ") & d == -1)
            {
                d = x;
                x = Editor.component.Length;
            }
        }
        string back = "1; 4B4B4B255";
        if (d != -1)
        {
            string BG = Editor.component[d].Replace("background = ", "");
            if (!string.IsNullOrEmpty(BG)) back = BG;
        }
        string[] Ar = back.Split(new string[1] { "; " }, System.StringSplitOptions.None);
        int selected = int.Parse(Ar[0]);

        Color32 color = Editeur.HexToColor(Ar[1]);
        for (int i = 0; i < go.childCount; i++)
        {
            go.GetChild(i).GetComponent<Image>().color = color;
            go.GetChild(i).GetComponent<UImage_Reader>().Type[0] = jsonData[selected].type[0];
            go.GetChild(i).GetComponent<UImage_Reader>().Load(sp[selected], null);
        }
        Vector2 size = sp[selected].Frames[0].Size();
        go.GetComponent<CanvasScaler>().referenceResolution = size;
        float match = 1;
        if (size.y < size.x) match = 0;
        go.GetComponent<CanvasScaler>().matchWidthOrHeight = match;
    }

    int j = 0;
    public void Page(int p)
    {
        int bgNumber = 12;
        int bgDisplayed = 6;

        j = j + p;

        for (int i = 0; i < bgDisplayed; i++)
        {
            if (sp[Selected].Frames.Length > 0)
                transform.GetChild(0).GetChild(i + 1).GetChild(0).GetComponent<Image>().sprite = sp[j + i].Frames[0];
        }

        transform.GetChild(0).GetChild(0).GetComponent<Button>().interactable = j > 0;
        transform.GetChild(0).GetChild(7).GetComponent<Button>().interactable = (j + bgDisplayed) < bgNumber;
    }

    public void ChangeColorPickerBG(GameObject BG)
    {
        CP.transform.GetChild(0).GetChild(1).GetComponent<HexColorField>().displayAlpha = false;
        CP.transform.GetChild(4).GetChild(3).gameObject.SetActive(false);
        BG.SetActive(true);
        if(sp[Selected].Frames.Length > 0) BG.GetComponent<Image>().sprite = sp[Selected].Frames[0];
    }
}
