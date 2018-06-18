using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using PlayerPrefs = PreviewLabs.PlayerPrefs;

public class Inventory : MonoBehaviour {

    int[] page;

    string[] article1;
    string[] article2;

    Sprite[] sp1;
    Sprite[] sp2;

    public string[] ids;
    
    bool Refreshed = false;
    public void Refresh()
    {
        if (!Refreshed)
            Actualse();
        Refreshed = true;

        RefreshArticle();
    }

    void Start () {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Flush();

        for (int i = 1; i < 3; i++)
        {
            if (PlayerPrefs.GetString("Item" + i) == "")
                PlayerPrefs.SetString("Item" + i, "0");
        }
        if(PlayerPrefs.GetInt("PlayerSkin") == new int())
            PlayerPrefs.SetInt("PlayerSkin", 0);
	}

    void Actualse()
    {
        page = new int[3];
        for (int v = 1; v <= 1; v++)
        {
            int cLenght = System.IO.Directory.GetFiles(Application.persistentDataPath + "/Textures/" + v + "/").Length;

            Sprite[] b = new Sprite[cLenght];
            string[] d = new string[cLenght];
            for (int i = 0; i < cLenght; i++)
            {
                byte[] Result2 = System.IO.File.ReadAllBytes(Application.persistentDataPath + "/Textures/" + v + "/" + i + ".png");
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(Result2);
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                b[i] = sprite;

                if (i < 5)
                {
                    transform.GetChild(0).GetChild(0).GetChild(2).GetChild(v - 1).GetChild(i + 1).GetChild(0).GetComponent<Image>().sprite = b[i];
                    transform.GetChild(0).GetChild(0).GetChild(2).GetChild(v - 1).GetChild(i + 1).GetChild(1).GetComponent<Text>().text = "";
                }

                transform.GetChild(0).GetChild(0).GetChild(2).GetChild(v - 1).GetChild(6).GetComponent<Button>().interactable = page[v] > 0;
                transform.GetChild(0).GetChild(0).GetChild(2).GetChild(v - 1).GetChild(7).GetComponent<Button>().interactable = page[v] + 5 < cLenght;

                if (v == 1)
                {
                    sp1 = b;
                    article1 = d;
                }
                else if (v == 2)
                {
                    sp2 = b;
                    article2 = d;
                }
            }

            RefreshArticle();
        }
    }

    public void PagePlus(int cat)
    {
        string[] c = new string[0];
        Sprite[] b = new Sprite[0];
        if (cat == 1)
        {
            c = article1;
            b = sp1;
        }
        else if (cat == 2)
        {
            c = article2;
            b = sp2;
        }

        page[cat] = page[cat] + 1;
        for (int i = 0; i < c.Length & i < 5; i++)
            transform.GetChild(0).GetChild(0).GetChild(2).GetChild(cat - 1).GetChild(i + 1).GetChild(0).GetComponent<Image>().sprite = b[i + page[cat]];

        transform.GetChild(0).GetChild(0).GetChild(2).GetChild(cat - 1).GetChild(6).GetComponent<Button>().interactable = page[cat] > 0;
        transform.GetChild(0).GetChild(0).GetChild(2).GetChild(cat - 1).GetChild(7).GetComponent<Button>().interactable = page[cat] + 5 < c.Length;

        RefreshArticle();
    }
    public void PageMoins(int cat)
    {
        string[] c = new string[0];
        Sprite[] b = new Sprite[0];
        if (cat == 1)
        {
            c = article1;
            b = sp1;
        }
        else if (cat == 2)
        {
            c = article2;
            b = sp2;
        }

        page[cat] = page[cat] - 1;
        for (int i = 0; i < c.Length & i < 5; i++)
            transform.GetChild(0).GetChild(0).GetChild(2).GetChild(cat - 1).GetChild(i + 1).GetChild(0).GetComponent<Image>().sprite = b[i + page[cat]];

        transform.GetChild(0).GetChild(0).GetChild(2).GetChild(cat - 1).GetChild(6).GetComponent<Button>().interactable = page[cat] > 0;
        transform.GetChild(0).GetChild(0).GetChild(2).GetChild(cat - 1).GetChild(7).GetComponent<Button>().interactable = page[cat] + 5 < c.Length;

        RefreshArticle();
    }

    public void RefreshArticle()
    {
        if (Refreshed)
        {
            for (int i = 0; i < 1; i++)
            {
                string PPname = "Item" + (i + 1);
                if (PlayerPrefs.GetString(PPname) == null)
                    PlayerPrefs.SetString(PPname, "0");
                string PP = PlayerPrefs.GetString(PPname);
                string[] a = PP.Split(new string[1] { " " }, StringSplitOptions.None);

                for (int v = 1; v < 6; v++)
                {
                    bool f = false;
                    for (int k = 0; k < a.Length; k++)
                    {
                        if (a[k].Contains((v - 1 + page[i + 1]).ToString()))
                            f = true;
                    }
                    transform.GetChild(0).GetChild(0).GetChild(2).GetChild(i).GetChild(v).GetComponent<Button>().interactable = f;

                    string t = LangueAPI.String(ids[0]);
                    if (f & PlayerPrefs.GetInt("PlayerSkin") == (v - 1) + page[i + 1])
                        t = LangueAPI.String(ids[1]);
                    else if (f)
                        t = "";

                    transform.GetChild(0).GetChild(0).GetChild(2).GetChild(i).GetChild(v).GetChild(1).GetComponent<Text>().text = t;
                }
            }
        }
    }

    public void Select(float cas)
    {
        int cat = (int)cas;
        int art = Mathf.RoundToInt((cas - (int)cas) * 10) + page[cat];

        if (cat == 1)
            PlayerPrefs.SetInt("PlayerSkin", art-1);

        RefreshArticle();
    }
}
