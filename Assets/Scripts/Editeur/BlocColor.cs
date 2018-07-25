using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlocColor : MonoBehaviour {

    public InputField[] RgbField;
    public Scrollbar[] RgbBar;
    byte[] RGB = new byte[4];

    public Editeur editeur;
    public int Bloc = -1;
    private void Start()
    {
        RGB = new byte[] { 255, 255, 255, 255 };
        RgbField[0].text = RGB[0].ToString();
        RgbField[1].text = RGB[1].ToString();
        RgbField[2].text = RGB[2].ToString();
        RgbField[3].text = RGB[3].ToString();
    }

    void Update () {
        if (Bloc != editeur.SelectedBlock)
        {
            Bloc = editeur.SelectedBlock;

            Color32 a = Editeur.HexToColor(editeur.GetBlocStatus(3, Bloc));
            RGB = new byte[] { a.r, a.g, a.b, a.a };

            for (int i = 0; i < RgbField.Length; i++)
            {
                RgbField[i].text = RGB[i].ToString();
                RgbBar[i].value = float.Parse(RGB[i].ToString()) / 255;
            }
        }
        else
        {
            for (int i = 0; i < RgbField.Length; i++)
            {
                if (RgbField[i].text != RgbBar[i].value.ToString() & RgbField[i].text == RGB[i].ToString())
                    RgbField[i].text = ((int)(RgbBar[i].value * 255)).ToString();

                if (RgbField[i].text != ((int)(RgbBar[i].value * 255)).ToString() & RgbField[i].text != "")
                    RgbBar[i].value = int.Parse(RgbField[i].text) / 255;
            }

            if (RgbField[0].text != "")
                RGB[0] = byte.Parse(RgbField[0].text);
            if (RgbField[1].text != "")
                RGB[1] = byte.Parse(RgbField[1].text);
            if (RgbField[2].text != "")
                RGB[2] = byte.Parse(RgbField[2].text);
            if (RgbField[3].text != "")
                RGB[3] = byte.Parse(RgbField[3].text);

            Color32 color = new Color32(RGB[0], RGB[1], RGB[2], RGB[3]);
            GetComponent<Image>().color = color;
            editeur.ChangBlocStatus(3, Editeur.ColorToHex(color), Bloc);
        }
    }
}
