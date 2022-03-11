using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AngryDash.Image;
using AngryDash.Image.Reader;
using AngryDash.Language;
using FileFormat;
using FileFormat.XML;
using Security;
using Tools;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Rect = UnityEngine.Rect;

[Serializable] //Display in Editor
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
    public static string xmlPath => Application.persistentDataPath + "/data.bin";

    /// <summary> Key used to encode the XML file </summary>
    public static string encodeKey = "CheatingIsBad,ItDoesNotHelpAnyoneAndYouWillNotFeelStrongerButMuchMoreDependentOnCheating." +
        "SoWhyTryToUnderstandThisKey?FindAnotherActivity,ThereIsSoMuchBetterToDo!YouAreStillHere?WhatAreYouWaitingFor?" +
        "DoYouWantToRuinUs?OrDoYouThinkOnlyOfYourselfAndTheRestHasNoImportance?";
    /// <summary> Get or Set the XML file </summary>
    public static RootElement xmlDefault
    {
        get
        {
            //Default value
            var XML = string.Join("",
            "<root>",
                "<Inventory>",
                    "<category name=\"native/PLAYERS/\">",
                        "<item name=\"0\" selected=\"\" />",
                    "</category>",
                    "<category name=\"native/TRACES/\">",
                        "<item name=\"0\" selected=\"\" />",
                    "</category>",
                "</Inventory>",
                "<Money>0</Money>",
                "<PlayedLevels type=\"Official\" />",
            "</root>");
            //If the file exist, load it
            if (File.Exists(xmlPath))
            {
                var decodeBinary = Binary.Parse(File.ReadAllText(xmlPath)).Decode(Encoding.UTF8);
                XML = Encrypting.Decrypt(decodeBinary, encodeKey);
            }
            return new XML(XML).RootElement;
        }
        set
        {
            var binary = new Binary(Encrypting.Encrypt(value.xmlFile.ToString(), encodeKey).ToByte(Encoding.UTF8));
            File.WriteAllText(xmlPath, binary.ToString());
        }
    }


    //Script Functions
    /// <summary> URL of the shop </summary>
    public string serverURL
    {
        get
        {
            var categoryName = category.Replace("native", "").Replace("/", "").ToLower();
            return "https://06games.ddns.net/Projects/Games/Angry%20Dash/shop/" + categoryName + ".xml";
        }
    }
    /// <summary> The category of item managed by this instance of the script </summary>
    public string category = "native/PLAYERS/";
    /// <summary> a string to add at the end of the file name when loading the preview </summary>
    public string fileSuffix = "";
    /// <summary> The selected item's index </summary>
    public int selected;
    /// <summary> List of all the category's items </summary>
    public InvItem[] items = new InvItem[0];

    private RootElement xml;


    private void Start()
    {
        xml = xmlDefault;
        StartCoroutine(Refresh());
    }

    /// <summary> Check item's price and setup the items array </summary>
    private IEnumerator Refresh()
    {
        using (var webRequest = UnityWebRequest.Get(serverURL))
        {
            webRequest.timeout = 10;
            yield return webRequest.SendWebRequest();

            var xmlResult = "";
            if (!string.IsNullOrEmpty(webRequest.error))
            {
                xmlResult = "<root><version v=\"0 - " + Application.version + "\">";
                foreach (var item in xml.GetItem("Inventory").GetItemByAttribute("category", "name", category).GetItems("item")) xmlResult += "<item name=\"" + item.Attribute("name") + "\" />";
                xmlResult += "</version></root>";
            }
            else xmlResult = webRequest.downloadHandler.text;

            var root = new XML(xmlResult).RootElement;
            Item ItemsRoot = null;
            foreach (var item in root.GetItems("version"))
            {
                var versions = item.Attribute("v").Split("-");
                if (versions.Length == 2)
                {
                    var actual = Versioning.Actual;
                    var old = new Versioning(versions[0]);
                    var newer = new Versioning(versions[1]);

                    //Check if the game version is between the defined versions
                    if (old.CompareTo(actual, Versioning.SortConditions.OlderOrEqual) & newer.CompareTo(actual, Versioning.SortConditions.NewerOrEqual))
                    {
                        ItemsRoot = item;
                        break;
                    }
                }
            }
            if (ItemsRoot != null)
            {
                var shopItem = ItemsRoot.GetItems("item");
                items = new InvItem[shopItem.Length];
                for (var i = 0; i < shopItem.Length; i++)
                {
                    if (!float.TryParse(shopItem[i].GetItem("price").Value, out var price)) price = -1;
                    var Name = shopItem[i].Attribute("name");
                    items[i] = new InvItem(Name, price);
                }

                Reload();
            }
        }
    }

    /// <summary> Display all items </summary>
    public void Reload()
    {
        transform.GetChild(0).GetChild(0).GetComponent<Text>().text = xml.GetItem("Money").Value;
        Transform content = transform.GetChild(1).GetComponent<ScrollRect>().content;
        for (var i = 1; i < content.childCount; i++)
            Destroy(content.GetChild(i).gameObject);

        selected = Array.IndexOf(items, items.SingleOrDefault(item => item.name == GetSelected(xml, category)));
        if (selected < 0 | selected > items.Length) selected = 0;
        else if (!Owned(items[selected].name))
        {
            xml.GetItem("Inventory").GetItemByAttribute("category", "name", category).GetItemByAttribute("item", "selected").Value = "0";
            selected = 0;
        }

        for (var i = 0; i < items.Length; i++)
        {
            var owned = Owned(items[i].name);

            var go = Instantiate(content.GetChild(0).gameObject, content).transform;


            var jsonData = JSON_API.Parse(category + items[i].name + fileSuffix);
            var data = new Sprite_API_Data[4];
            var tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(jsonData.textures[0].path));
            tex = tex.PremultiplyAlpha();
            tex.Apply();
            data[0] = new Sprite_API_Data
            {
                Frames = new List<Sprite> { Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, jsonData.textures[0].border) },
                Delay = new List<float> { 1 },
                Repeat = 1
            }; //Basic is default png image
            data[1] = Sprite_API.GetSprites(jsonData.textures[0].path); //Basic replace hover
            jsonData.textures[1].display = jsonData.textures[0].display;
            go.GetChild(0).GetComponent<UImage_Reader>().Load(data).ApplyJson(jsonData).StopAnimating(0, false);

            go.GetComponent<Button>().interactable = i != selected;
            var button = i;
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
                else if (items[i].price > 0)
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
    public bool Owned(string name) { return Owned(xml, category, name); }
    /// <summary> Check if the item is owned </summary>
    /// <param name="xml">Xml file</param>
    /// <param name=" category">Item's category</param>
    /// <param name="name">Item's name</param>
    public static bool Owned(RootElement xml, string category, string name) { return xml.GetItem("Inventory").GetItemByAttribute("category", "name", category).GetItemByAttribute("item", "name", name).Exist; }

    /// <summary> Buy the item </summary>
    /// <param name="index">Item's index</param>
    public void Buy(int index)
    {
        var money = int.Parse(xml.GetItem("Money").Value);
        if (items[index].price <= money)
        {
            if (!xml.GetItem("Inventory").GetItemByAttribute("category", "name", category).Exist) xml.GetItem("Inventory").CreateItem("category").SetAttribute("name", category);
            xml.GetItem("Inventory").GetItemByAttribute("category", "name", category).CreateItem("item").SetAttribute("name", items[index].name);
            xml.GetItem("Money").Value = (money - items[index].price).ToString();
            Social.IncrementEvent("CgkI9r-go54eEAIQCA", (uint)items[index].price); //Statistics about coin expenses
            Reload();
        }
        xmlDefault = xml;
    }

    /// <summary> Select the item </summary>
    /// <param name="index">Item's index</param>
    public void Select(int index)
    {
        selected = index;
        var cat = xml.GetItem("Inventory").GetItemByAttribute("category", "name", category);
        cat.GetItemsByAttribute("item", "selected").ForEach(i => i.RemoveAttribute("selected")); //Unselects
        cat.GetItemByAttribute("item", "name", items[index].name).SetAttribute("selected"); //Selects
        Reload();
        xmlDefault = xml;
    }

    public static string GetSelected(RootElement xml, string category) { return xml.GetItem("Inventory").GetItemByAttribute("category", "name", category).GetItemByAttribute("item", "selected").Attribute("name"); }
}
