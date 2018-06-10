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
        Transform _Color = transform.GetChild(1);
        if (file != Editor.file)
        {
            Selected = int.Parse(Editor.component[4].Split(new string[1] { "; " }, System.StringSplitOptions.None)[0]);
            Color32 rgb = Editeur.HexToColor(Editor.component[4].Split(new string[1] { "; " }, System.StringSplitOptions.None)[1]);
            _Color.GetChild(0).GetComponent<InputField>().text = rgb.r.ToString();
            _Color.GetChild(1).GetComponent<InputField>().text = rgb.g.ToString();
            _Color.GetChild(2).GetComponent<InputField>().text = rgb.b.ToString();
            Page(0);
        }

        file = Editor.file;
        if (Editor.file != "")
        {
            
            byte r = 0;
            byte g = 0;
            byte b = 0;

            if (int.Parse(_Color.GetChild(0).GetComponent<InputField>().text) > 255)
                _Color.GetChild(0).GetComponent<InputField>().text = "255";
            if (int.Parse(_Color.GetChild(1).GetComponent<InputField>().text) > 255)
                _Color.GetChild(1).GetComponent<InputField>().text = "255";
            if (int.Parse(_Color.GetChild(2).GetComponent<InputField>().text) > 255)
                _Color.GetChild(2).GetComponent<InputField>().text = "255";

            if (int.Parse(_Color.GetChild(0).GetComponent<InputField>().text) < 0)
                _Color.GetChild(0).GetComponent<InputField>().text = "0";
            if (int.Parse(_Color.GetChild(1).GetComponent<InputField>().text) < 0)
                _Color.GetChild(1).GetComponent<InputField>().text = "0";
            if (int.Parse(_Color.GetChild(2).GetComponent<InputField>().text) < 0)
                _Color.GetChild(2).GetComponent<InputField>().text = "0";


            if (_Color.GetChild(0).GetComponent<InputField>().text != "")
                r = byte.Parse(_Color.GetChild(0).GetComponent<InputField>().text);
            if (_Color.GetChild(1).GetComponent<InputField>().text != "")
                g = byte.Parse(_Color.GetChild(1).GetComponent<InputField>().text);
            if (_Color.GetChild(2).GetComponent<InputField>().text != "")
                b = byte.Parse(_Color.GetChild(2).GetComponent<InputField>().text);
            

            Color32 color = new Color32(r, g, b, 255);

            for (int i = 0; i < transform.GetChild(0).childCount-2; i++)
            {
                Transform trans = transform.GetChild(0).GetChild(i+1);
                trans.GetChild(0).GetComponent<Image>().color = color;

                if (i+j == Selected)
                    trans.GetComponent<Image>().color = new Color32(210, 210, 210, 255);
                else trans.GetComponent<Image>().color = new Color32(92, 92, 92, 255);
            }

            Editor.component[4] = Selected + "; " + Editeur.ColorToHex(color);

            ActualiseFond(Editor);
        }
	}

    public void ChangFond(int i) { Selected = i+j; }

    public void ActualiseFond(Editeur Editor)
    {
        Transform go = GameObject.Find("BackgroundDiv").transform;
        for (int i = 0; i < go.childCount; i++)
        {
            Image Im = go.GetChild(i).GetComponent<Image>();
            string[] Ar = Editor.component[4].Split(new string[1] { "; " }, System.StringSplitOptions.None);
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
}
