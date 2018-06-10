using UnityEngine;
using System;
using UnityEngine.UI;
using PlayerPrefs = PreviewLabs.PlayerPrefs;
using System.Net;

public class Shop : MonoBehaviour {
    
    int money;
    int[] page;

    string[] article1;
    string[] article2;

    Sprite[] sp1;
    Sprite[] sp2;
    

    bool Refreshed = false;
    public void Refresh()
    {
        if (InternetAPI.IsConnected())
        {
            if (!Refreshed)
                ArticleInitialise();
            Refreshed = true;

            transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        }
        else if (Refreshed)
            transform.GetChild(0).GetChild(0).gameObject.SetActive(true);
        else transform.GetChild(0).GetChild(2).gameObject.SetActive(true);
    }

    private void Update()
    {
        money = PlayerPrefs.GetInt("money");
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
            transform.GetChild(0).GetChild(i).GetChild(1).GetChild(0).GetComponent<Text>().text = money.ToString();
    }

    public void Acheter(float F)
    {
        int cat = (int)F;
        int art = Mathf.RoundToInt((F - (int)F) * 10)+page[cat];

        string[] b = new string[0];
        if (cat == 1)
            b = article1;
        else if (cat == 2)
            b = article2;
        
        if (b.Length != 0)
        {
            string[] a = b[art-1].Split(new string[1] { "; " }, StringSplitOptions.None);
            int price = int.Parse(a[1]);

            if (money >= price)
            {
                PlayerPrefs.SetInt("money", money - price);
                PlayerPrefs.SetString("Item" + cat, PlayerPrefs.GetString("Item" + cat) + " " + art);
                print("Payé, article : " + cat + ":" + art + "\nFile :  " + PlayerPrefs.GetString("Item" + cat));

                RefreshArticle();
            }
            else print("Trop chère");
        }
    }

    void RefreshArticle()
    {
        for(int i = 0; i < 2; i++)
        {
            string PPname = "Item" + (i + 1);
            if (PlayerPrefs.GetString(PPname) == null)
                PlayerPrefs.SetString(PPname, "0");
            string PP = PlayerPrefs.GetString(PPname);
            string[] a = PP.Split(new string[1] { " " }, StringSplitOptions.None);

            for(int v = 1; v < 6; v++)
            {
                bool f = true;
                for(int k = 0; k < a.Length; k++)
                {
                    if (a[k].Contains((v+page[i+1]).ToString()))
                        f = false;
                }
                transform.GetChild(0).GetChild(0).GetChild(2).GetChild(i).GetChild(v).GetComponent<Button>().interactable = f;
            }
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
        {
            transform.GetChild(0).GetChild(0).GetChild(2).GetChild(cat - 1).GetChild(i + 1).GetChild(0).GetComponent<Image>().sprite = b[i+page[cat]];
            transform.GetChild(0).GetChild(0).GetChild(2).GetChild(cat - 1).GetChild(i + 1).GetChild(1).GetComponent<Text>().text = c[i + page[cat]].Split(new string[1] { "; " }, StringSplitOptions.None)[1];
        }

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
        {
            transform.GetChild(0).GetChild(0).GetChild(2).GetChild(cat - 1).GetChild(i + 1).GetChild(0).GetComponent<Image>().sprite = b[i + page[cat]];
            transform.GetChild(0).GetChild(0).GetChild(2).GetChild(cat - 1).GetChild(i + 1).GetChild(1).GetComponent<Text>().text = c[i + page[cat]].Split(new string[1] { "; " }, StringSplitOptions.None)[1];
        }

        transform.GetChild(0).GetChild(0).GetChild(2).GetChild(cat - 1).GetChild(6).GetComponent<Button>().interactable = page[cat] > 0;
        transform.GetChild(0).GetChild(0).GetChild(2).GetChild(cat - 1).GetChild(7).GetComponent<Button>().interactable = page[cat] + 5 < c.Length;

        RefreshArticle();
    }

    void ArticleInitialise()
    {
        page = new int[3];
        for (int v = 1; v <= 1; v++)
        {
            string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/shop/items_" + v + ".txt";
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            string Result = client.DownloadString(URL);
            string[] c = Result.Split(new string[1] { "\n" }, StringSplitOptions.None);

            Sprite[] b = new Sprite[c.Length];
            for (int i = 0; i < c.Length; i++)
            {
                string[] a = c[i].Split(new string[1] { "; " }, StringSplitOptions.None)[0].Split(new string[1] { ":" }, StringSplitOptions.None);
                byte[] Result2 = System.IO.File.ReadAllBytes(Application.persistentDataPath + "/Textures/" + a[0] + "/" + a[1] + ".png");
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(Result2);
                Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                b[i] = sprite;

                if (i < 5)
                {
                    transform.GetChild(0).GetChild(0).GetChild(2).GetChild(v - 1).GetChild(i + 1).GetChild(0).GetComponent<Image>().sprite = b[i];
                    transform.GetChild(0).GetChild(0).GetChild(2).GetChild(v - 1).GetChild(i + 1).GetChild(1).GetComponent<Text>().text = c[i].Split(new string[1] { "; " }, StringSplitOptions.None)[1];
                }

                transform.GetChild(0).GetChild(0).GetChild(2).GetChild(v - 1).GetChild(6).GetComponent<Button>().interactable = page[v] > 0;
                transform.GetChild(0).GetChild(0).GetChild(2).GetChild(v - 1).GetChild(7).GetComponent<Button>().interactable = page[v]+5 < c.Length;

            }

            if (v == 1)
            {
                sp1 = b;
                article1 = c;
            }
            else if (v == 2)
            {
                sp2 = b;
                article2 = c;
            }
        }
        
        RefreshArticle();
    }
    
    public void StartPub()
    {
#if UNITY_EDITOR
        PlayerPrefs.SetInt("money", money + 500);
#else
        //AdManager.RewardAdd.ShowRewardedVideoButtonClicked();
#endif
    }
}
