using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Background : MonoBehaviour {

    public int Selected;
    public Editeur Editor;
    string file;
    Sprite[] sp;

    public ColorPicker CP;

    private void Start()
    {
        if (string.IsNullOrEmpty(Editor.file)) gameObject.SetActive(false);
    }

    public void Charg()
    {
            File.WriteAllBytes(Application.persistentDataPath + "/Textures/2/0.png", Texture2D.whiteTexture.EncodeToPNG());

        if (sp == null)
        {
            int f = Directory.GetFiles(Application.persistentDataPath + "/Textures/2/").Length;
            sp = new Sprite[f];

            for (int i = 0; i < f; i++)
            {
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(File.ReadAllBytes(Application.persistentDataPath + "/Textures/2/" + i + ".png"));
                sp[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
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
                    d = x;
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
                    d = x;
            }
            if(d != -1)
                Editor.component[d] = "background = " + Selected.ToString("0") + "; " + Editeur.ColorToHex(color);

            ActualiseFond(Editor);
        }
	}

    public void ChangFond(int i) { Selected = i+j; }

    public void ActualiseFond(Editeur Editor)
    {
        Charg();
        Transform go = GameObject.Find("BackgroundDiv").transform;

        int d = -1;
        for (int x = 0; x < Editor.component.Length; x++)
        {
            if (Editor.component[x].Contains("background = ") & d == -1)
                d = x;
        }
        string back = "1; 4B4B4B255";
        if (d != -1)
            back = Editor.component[d].Replace("background = ", "");
        string[] Ar = back.Split(new string[1] { "; " }, System.StringSplitOptions.None);

        for (int i = 0; i < go.childCount-1; i++)
        {
            Image Im = go.GetChild(i).GetComponent<Image>();
            Im.sprite = sp[int.Parse(Ar[0])];
            Im.color = Editeur.HexToColor(Ar[1]);
        }
    }

    int j = 0;
    public void Page(int p)
    {
        j = j + p;

        for (int i = 0; i < 6; i++)
        {

            transform.GetChild(0).GetChild(i + 1).GetChild(0).GetComponent<Image>().sprite = sp[j + i];
        }

        transform.GetChild(0).GetChild(0).GetComponent<Button>().interactable = j > 0;
        transform.GetChild(0).GetChild(7).GetComponent<Button>().interactable = (j + 6) < sp.Length;
    }

    public void ChangeColorPickerBG(GameObject BG)
    {
        CP.transform.GetChild(0).GetChild(1).GetComponent<HexColorField>().displayAlpha = false;
        CP.transform.GetChild(4).GetChild(3).gameObject.SetActive(false);
        BG.SetActive(true);
        BG.GetComponent<Image>().sprite = sp[Selected];
    }
}
