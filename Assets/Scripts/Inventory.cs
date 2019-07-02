using AngryDash.Image.Reader;
using AngryDash.Language;
using FileFormat;
using System.Collections;
using System.Linq;
using System.Net;
using Tools;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable] //Display in Editor
public class InvItem
{
    public string name;
    public float price;
    public InvItem(string Name, float Price = 0) { name = Name; price = Price; }
}

public class Inventory : MonoBehaviour
{
    //General Functions
    /// <summary> Path to the XML file </summary>
    public static string xmlPath { get { return Application.persistentDataPath + "/data.bin"; } }
    /// <summary> Key used to encode the XML file </summary>
    public static string encodeKey = "CheatingIsBad,ItDoesNotHelpAnyoneAndYouWillNotFeelStrongerButMuchMoreDependentOnCheating." +
        "SoWhyTryToUnderstandThisKey?FindAnotherActivity,ThereIsSoMuchBetterToDo!YouAreStillHere?WhatAreYouWaitingFor?" +
        "DoYouWantToRuinUs?OrDoYouThinkOnlyOfYourselfAndTheRestHasNoImportance?";
    /// <summary> Get or Set the XML file </summary>
    public static FileFormat.XML.RootElement xmlDefault
    {
        get
        {
            //Default value
            string XML = string.Join("",
            "<root>",
                "<OwnedItems>",
                    "<item name=\"0\" />",
                "</OwnedItems>",
                "<SelectedItems>",
                    "<item category=\"native/PLAYERS/\">0</item>",
                    "<item category=\"native/TRACES/\">0</item>",
                "</SelectedItems>",
                "<Money>0</Money>",
                "<PlayedLevels type=\"Official\" />",
            "</root>");
            //If the file exist, load it
            if (System.IO.File.Exists(xmlPath))
            {
                string decodeBinary = Binary.Parse(System.IO.File.ReadAllText(xmlPath)).Decode(System.Text.Encoding.UTF8);
                XML = Security.Encrypting.Decrypt(decodeBinary, encodeKey);
            }
            return new FileFormat.XML.XML(XML).RootElement;
        }
        set
        {
            Binary binary = new Binary(Security.Encrypting.Encrypt(value.xmlFile.ToString(), encodeKey).ToByte(System.Text.Encoding.UTF8));
            System.IO.File.WriteAllText(xmlPath, binary.ToString());
        }
    }


    //Script Functions
    /// <summary> URL of the shop </summary>
    public string serverURL
    {
        get
        {
            string categoryName = category.Replace("native", "").Replace("/", "").ToLower();
            return "https://06games.ddns.net/Projects/Games/Angry%20Dash/shop/" + categoryName + ".xml";
        }
    }
    /// <summary> The category of item managed by this instance of the script </summary>
    public string category = "native/PLAYERS/";
    /// <summary> a string to add at the end of the file name when loading the preview </summary>
    public string fileSuffix = "";
    /// <summary> The selected item's index </summary>
    public int selected = 0;
    /// <summary> List of all the category's items </summary>
    public InvItem[] items = new InvItem[0];
    FileFormat.XML.RootElement xml;


    private void Start()
    {
        if (!InternetAPI.IsConnected())
        {
            transform.parent.GetComponent<MenuManager>().Array(0);
            return; //If no connection then stop loading
        }
        xml = xmlDefault;
        Refresh();
    }

    /// <summary> Check item's price and setup the items array </summary>
    public void Refresh()
    {
        WebClient client = new WebClient();
        client.Encoding = System.Text.Encoding.UTF8;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        string Result = null;
        try { Result = client.DownloadString(serverURL); }
        catch
        {
            transform.parent.GetComponent<MenuManager>().Array(0);
            return; //If no connection then stop loading
        }

        FileFormat.XML.RootElement root = new FileFormat.XML.XML(Result).RootElement;
        FileFormat.XML.Item ItemsRoot = root.GetItemByAttribute("version", "v", Application.version);
        FileFormat.XML.Item[] shopItem = ItemsRoot.GetItems("item");
        items = new InvItem[shopItem.Length];
        for (int i = 0; i < shopItem.Length; i++)
        {
            float price = 0;
            float.TryParse(shopItem[i].Value, out price);
            string Name = shopItem[i].Attribute("name");
            items[i] = new InvItem(Name, price);
        }

        Reload();
    }

    /// <summary> Display all items </summary>
    public void Reload()
    {
        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = xml.GetItem("Money").Value;
        Transform content = transform.GetChild(1).GetComponent<ScrollRect>().content;
        for (int i = 1; i < content.childCount; i++)
            Destroy(content.GetChild(i).gameObject);

        FileFormat.XML.Item xmlItem = xml.GetItem("SelectedItems").GetItemByAttribute("item", "category", category);
        if (xmlItem == null)
        {
            xmlItem = xml.GetItem("SelectedItems").CreateItem("item");
            xmlItem.CreateAttribute("category", category);
        }

        string xmlSelected = xml.GetItem("SelectedItems").GetItemByAttribute("item", "category", category).value<string>();
        selected = System.Array.IndexOf(items, items.SingleOrDefault(item => item.name == xmlSelected));
        if (selected < 0 | selected > items.Length) selected = 0;
        else if (!Owned(items[selected].name))
        {
            xml.GetItem("SelectedItems").GetItemByAttribute("item", "category", category).Value = "0";
            selected = 0;
        }

        for (int i = 0; i < items.Length; i++)
        {
            bool owned = Owned(items[i].name);

            Transform go = Instantiate(content.GetChild(0).gameObject, content).transform;


            AngryDash.Image.JSON_PARSE_DATA jsonData = AngryDash.Image.JSON_API.Parse(category + items[i].name + fileSuffix);
            AngryDash.Image.Sprite_API_Data[] data = new AngryDash.Image.Sprite_API_Data[4];
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(System.IO.File.ReadAllBytes(jsonData.path[0]));
            tex = Texture2DExtensions.PremultiplyAlpha(tex);
            tex.Apply();
            data[0] = new AngryDash.Image.Sprite_API_Data()
            {
                Frames = new Sprite[] { Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, jsonData.border[0]) },
                Delay = new float[] { 1 },
                Repeat = 1
            }; //Basic is default png image
            data[1] = AngryDash.Image.Sprite_API.GetSprites(jsonData.path[0], jsonData.border[0]); //Basic replace hover
            jsonData.type[1] = jsonData.type[0];
            go.GetChild(0).GetComponent<UImage_Reader>().Load(data).ApplyJson(jsonData).StopAnimating(0, false);

            go.GetComponent<Button>().interactable = i != selected;
            int button = i;
            go.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (owned) Select(button);
                else Buy(button);
            });
            go.GetChild(1).gameObject.SetActive(!owned);
            go.GetChild(2).gameObject.SetActive(owned & selected == i);
            if (!owned)
            {
                go.GetChild(1).gameObject.SetActive(true);
                go.GetChild(2).gameObject.SetActive(false);

                if (items[i].price == 0)
                {
                    go.GetChild(1).GetChild(0).GetComponent<Text>().text = LangueAPI.Get("native", "InventoryItemFree", "Free");
                    go.GetChild(1).GetChild(0).GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
                }
                else
                {
                    go.GetChild(1).GetChild(0).GetComponent<Text>().text = items[i].price.ToString();
                    go.GetChild(1).GetChild(0).GetComponent<Text>().alignment = TextAnchor.MiddleRight;
                }
                go.GetChild(1).GetChild(1).gameObject.SetActive(items[i].price > 0);
            }
            go.gameObject.SetActive(true);
        }
    }

    /// <summary> Check if the item is owned </summary>
    /// <param name="name">Item's name</param>
    public bool Owned(string name) { return xml.GetItem("OwnedItems").GetItemByAttribute("item", "name", name) != null; }
    /// <summary> Check if the item is owned </summary>
    /// <param name="xml">Xml file</param>
    /// <param name="name">Item's name</param>
    public static bool Owned(FileFormat.XML.RootElement xml, string name)
    { return xml.GetItem("OwnedItems").GetItemByAttribute("item", "name", name) != null; }

    /// <summary> Buy the item </summary>
    /// <param name="index">Item's index</param>
    public void Buy(int index)
    {
        int money = int.Parse(xml.GetItem("Money").Value);
        if (items[index].price <= money)
        {
            xml.GetItem("OwnedItems").CreateItem("item").CreateAttribute("name", items[index].name);
            xml.GetItem("Money").Value = (money - items[index].price).ToString();
            Reload();
        }
        xmlDefault = xml;
    }

    /// <summary> Select the item </summary>
    /// <param name="index">Item's index</param>
    public void Select(int index)
    {
        selected = index;
        xml.GetItem("SelectedItems").GetItemByAttribute("item", "category", category).Value = items[index].name;
        Reload();
        xmlDefault = xml;
    }
}
