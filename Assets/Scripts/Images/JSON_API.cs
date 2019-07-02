using LibAPNG;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AngryDash.Image
{
    public class JSON_API
    {
        public static JSON_PARSE_DATA Parse(string baseID, FileFormat.JSON json = null, bool path = false)
        {
            CacheManager.Cache cache = new CacheManager.Cache("Ressources/textures/json");
            if (!cache.ValueExist(baseID))
            {
                if (json == null) NewJSON();
                else if (json.jToken == null) NewJSON();
                void NewJSON()
                {
                    json = new FileFormat.JSON("");
                    string jsonID = baseID + ".json";
                    if (!path)
                    {
                        string rpPath = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/";
                        if (File.Exists(rpPath + jsonID) | File.Exists(rpPath + baseID + " basic.png"))
                            jsonID = rpPath + baseID + ".json";
                        else jsonID = Application.persistentDataPath + "/Ressources/default/textures/" + baseID + ".json";
                    }
                    if (File.Exists(jsonID)) json = new FileFormat.JSON(File.ReadAllText(jsonID));
                }

                cache.Set(baseID, LoadParse(baseID, json, path));
            }
            return cache.Get<JSON_PARSE_DATA>(baseID);
        }
        public static JSON_PARSE_DATA LoadParse(string baseID, FileFormat.JSON json, bool path)
        {
            JSON_PARSE_DATA data = new JSON_PARSE_DATA();

            for (int jNum = 0; jNum < 2; jNum++)
            {
                string ID = baseID;
                if (jNum == 1)
                {
                    if (json.ValueExist("parent"))
                    {
                        ID = json.Value<string>("parent");
                        string jsonID = ID + ".json";
                        string rpPath = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/";
                        if (File.Exists(rpPath + jsonID) | File.Exists(rpPath + ID + " basic.png"))
                            jsonID = rpPath + ID + ".json";
                        else jsonID = Application.persistentDataPath + "/Ressources/default/textures/" + ID + ".json";
                        if (File.Exists(jsonID)) json = new FileFormat.JSON(File.ReadAllText(jsonID));
                    }
                    else return data;
                }

                //Textures
                if (true)
                {
                    FileFormat.JSON category = json.GetCategory("textures");

                    string[] paramNames = new string[] { "basic", "hover", "pressed", "disabled" };
                    if (data.path == null) data.path = new string[paramNames.Length];
                    if (data.border == null) data.border = new Vector4[paramNames.Length];
                    if (data.type == null) data.type = new int[paramNames.Length];
                    for (int i = 0; i < paramNames.Length; i++)
                    {
                        if (string.IsNullOrEmpty(data.path[i]) | !File.Exists(data.path[i]))
                        {
                            if (path) data.path[i] = ID + " " + paramNames[i] + ".png";
                            else data.path[i] = Sprite_API.spritesPath(ID + " " + paramNames[i] + ".png");
                        }

                        FileFormat.JSON paramCategory = category.GetCategory(paramNames[i]);
                        if (paramCategory.ContainsValues)
                        {
                            //Border
                            FileFormat.JSON borderCategory = paramCategory.GetCategory("border");
                            if (borderCategory.ContainsValues & data.border[i] == default)
                            {
                                if (borderCategory.ValueExist("left")) data.border[i].x = borderCategory.Value<float>("left");
                                if (borderCategory.ValueExist("right")) data.border[i].z = borderCategory.Value<float>("right");
                                if (borderCategory.ValueExist("top")) data.border[i].w = borderCategory.Value<float>("top");
                                if (borderCategory.ValueExist("bottom")) data.border[i].y = borderCategory.Value<float>("bottom");
                            }

                            //Path
                            if (string.IsNullOrEmpty(data.path[i]) & paramCategory.ValueExist("path"))
                            {
                                if (path) data.path[i] = new FileInfo(ID + ".json").Directory.FullName +
                                        "/" + paramCategory.Value<string>("path");
                                else data.path[i] = new FileInfo(Sprite_API.spritesPath(ID + ".json")).Directory.FullName +
                                        "/" + paramCategory.Value<string>("path");
                            }

                            if (paramCategory.ValueExist("type") & data.type[i] == default)
                            {
                                string ImageType = paramCategory.Value<string>("type");
                                if (ImageType == "Simple") data.type[i] = 0;
                                else if (ImageType == "Sliced") data.type[i] = 1;
                                else if (ImageType == "Tiled") data.type[i] = 2;
                            }
                        }
                    }
                }

                //Text
                if (true)
                {
                    FileFormat.JSON category = json.GetCategory("text");
                    //Color
                    Color32 textColor = new Color32(255, 255, 255, 255);
                    if (category.ValueExist("color")) HexColorField.HexToColor(category.Value<string>("color"), out textColor);
                    if (data.textColor.Equals(new Color32(255, 255, 255, 255))) data.textColor = textColor;

                    //Font Style
                    if (category.ValueExist("fontStyle") & data.textStyle == FontStyle.Normal)
                    {
                        string value = category.Value<string>("fontStyle");
                        if (value == "Normal") data.textStyle = FontStyle.Normal;
                        else if (value == "Bold") data.textStyle = FontStyle.Bold;
                        else if (value == "Italic") data.textStyle = FontStyle.Italic;
                        else if (value == "BoldAndItalic") data.textStyle = FontStyle.BoldAndItalic;
                    }
                    else data.textStyle = FontStyle.Normal;

                    //Font Alignment
                    FileFormat.JSON fontAlignment = category.GetCategory("fontAlignment");
                    if (fontAlignment.ContainsValues)
                    {
                        int horizontal = 0;
                        int vertical = 0;

                        if (fontAlignment.ValueExist("horizontal"))
                        {
                            string horizontalValue = fontAlignment.Value<string>("horizontal");
                            if (horizontalValue == "Left") horizontal = 0;
                            else if (horizontalValue == "Center") horizontal = 1;
                            else if (horizontalValue == "Right") horizontal = 2;
                        }

                        if (fontAlignment.ValueExist("vertical"))
                        {
                            string verticalValue = fontAlignment.Value<string>("vertical");
                            if (verticalValue == "Upper") vertical = 0;
                            else if (verticalValue == "Middle") vertical = 1;
                            else if (verticalValue == "Lower") vertical = 2;
                        }

                        if (data.textAnchor == TextAnchor.MiddleLeft) data.textAnchor = (TextAnchor)((vertical * 3) + horizontal);
                    }
                    else data.textAnchor = TextAnchor.MiddleLeft;

                    //Font Size
                    FileFormat.JSON fontSize = category.GetCategory("resize");
                    if (fontSize.ValueExist("minSize") & fontSize.ValueExist("maxSize")) data.textResize = true;
                    else { data.textResize = false; data.textSize = 14; }
                    if (fontSize.ValueExist("minSize")) data.textResizeMinAndMax[0] = fontSize.Value<int>("minSize");
                    if (fontSize.ValueExist("maxSize")) data.textResizeMinAndMax[1] = fontSize.Value<int>("maxSize");
                }
            }
            return data;
        }
    }
}
