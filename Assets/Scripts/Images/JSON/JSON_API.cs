using System;
using System.IO;
using AngryDash.Image.JSON;
using UnityEngine;

namespace AngryDash.Image
{
    public class JSON_API
    {
        public static Data Parse(string baseID, FileFormat.JSON json = null, bool path = false)
        {
            var jsonID = baseID + ".json";
            var rpPath = Sprite_API.forceRP + "textures/";
            if (!string.IsNullOrWhiteSpace(Sprite_API.forceRP) && (File.Exists(rpPath + jsonID) | File.Exists(rpPath + baseID + " basic.png")))
            {
                jsonID = rpPath + baseID + ".json";
                if (File.Exists(jsonID)) json = new FileFormat.JSON(File.ReadAllText(jsonID));
                else json = new FileFormat.JSON("");
                return LoadParse(baseID, json, path);
            }

            var cache = Cache.Open("Ressources/textures/json");
            if (!cache.ValueExist(baseID))
            {
                if (json == null) NewJSON();
                else if (json.jToken == null) NewJSON();
                void NewJSON()
                {
                    json = new FileFormat.JSON("");
                    if (!path)
                    {
                        rpPath = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/";
                        if (File.Exists(rpPath + jsonID) | File.Exists(rpPath + baseID + " basic.png")) jsonID = rpPath + baseID + ".json";
                        else jsonID = Application.persistentDataPath + "/Ressources/default/textures/" + baseID + ".json";
                    }
                    if (File.Exists(jsonID)) json = new FileFormat.JSON(File.ReadAllText(jsonID));
                }

                cache.Set(baseID, LoadParse(baseID, json, path));
            }
            return cache.Get<Data>(baseID);
        }
        public static Data LoadParse(string baseID, FileFormat.JSON json, bool path)
        {
            var data = new Data();

            for (var jNum = 0; jNum < 2; jNum++)
            {
                var ID = baseID;
                if (jNum == 1)
                {
                    if (json.ValueExist("parent"))
                    {
                        ID = json.Value<string>("parent");
                        var jsonID = ID + ".json";
                        var rpPath = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/";
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
                    var category = json.GetCategory("textures");

                    var paramNames = new[] { "basic", "hover", "pressed", "disabled" };
                    if (data.textures == null) data.textures = new JSON.Texture[paramNames.Length];
                    for (var i = 0; i < paramNames.Length; i++)
                    {
                        if (data.textures[i] == null) data.textures[i] = new JSON.Texture();
                        data.textures[i].type = (JSON.Texture.Type)i;
                        if (string.IsNullOrEmpty(data.textures[i].path) | !File.Exists(data.textures[i].path))
                        {
                            if (path) data.textures[i].path = ID + " " + paramNames[i] + ".png";
                            else data.textures[i].path = Sprite_API.spritesPath(ID + " " + paramNames[i] + ".png");
                        }

                        var paramCategory = category.GetCategory(paramNames[i]);
                        if (paramCategory.ContainsValues)
                        {
                            //Border
                            var borderCategory = paramCategory.GetCategory("border");
                            if (borderCategory.ContainsValues & data.textures[i].border == default)
                            {
                                if (borderCategory.ValueExist("left")) data.textures[i].border.x = borderCategory.Value<float>("left");
                                if (borderCategory.ValueExist("right")) data.textures[i].border.z = borderCategory.Value<float>("right");
                                if (borderCategory.ValueExist("top")) data.textures[i].border.w = borderCategory.Value<float>("top");
                                if (borderCategory.ValueExist("bottom")) data.textures[i].border.y = borderCategory.Value<float>("bottom");
                            }

                            //Path
                            if (string.IsNullOrEmpty(data.textures[i].path) & paramCategory.ValueExist("path"))
                            {
                                if (path) data.textures[i].path = new FileInfo(ID + ".json").Directory.FullName + "/" + paramCategory.Value<string>("path");
                                else data.textures[i].path = new FileInfo(Sprite_API.spritesPath(ID + ".json")).Directory.FullName + "/" + paramCategory.Value<string>("path");
                            }

                            if (paramCategory.ValueExist("type")) Enum.TryParse(paramCategory.Value<string>("type"), out data.textures[i].display);
                        }
                    }
                }

                //Text
                if (true)
                {
                    var category = json.GetCategory("text");
                    //Color
                    var textColor = new Color32(255, 255, 255, 255);
                    if (category.ValueExist("color")) HexColorField.HexToColor(category.Value<string>("color"), out textColor);
                    if (data.textColor.Equals(new Color32(255, 255, 255, 255))) data.textColor = textColor;

                    //Font Style
                    if (category.ValueExist("fontStyle") & data.textStyle == FontStyle.Normal)
                    {
                        var value = category.Value<string>("fontStyle");
                        if (value == "Normal") data.textStyle = FontStyle.Normal;
                        else if (value == "Bold") data.textStyle = FontStyle.Bold;
                        else if (value == "Italic") data.textStyle = FontStyle.Italic;
                        else if (value == "BoldAndItalic") data.textStyle = FontStyle.BoldAndItalic;
                    }
                    else data.textStyle = FontStyle.Normal;

                    //Font Alignment
                    var fontAlignment = category.GetCategory("fontAlignment");
                    if (fontAlignment.ContainsValues)
                    {
                        var horizontal = 0;
                        var vertical = 0;

                        if (fontAlignment.ValueExist("horizontal"))
                        {
                            var horizontalValue = fontAlignment.Value<string>("horizontal");
                            if (horizontalValue == "Left") horizontal = 0;
                            else if (horizontalValue == "Center") horizontal = 1;
                            else if (horizontalValue == "Right") horizontal = 2;
                        }

                        if (fontAlignment.ValueExist("vertical"))
                        {
                            var verticalValue = fontAlignment.Value<string>("vertical");
                            if (verticalValue == "Upper") vertical = 0;
                            else if (verticalValue == "Middle") vertical = 1;
                            else if (verticalValue == "Lower") vertical = 2;
                        }

                        if (data.textAnchor == TextAnchor.MiddleLeft) data.textAnchor = (TextAnchor)((vertical * 3) + horizontal);
                    }
                    else data.textAnchor = TextAnchor.MiddleLeft;

                    //Font Size
                    var fontSize = category.GetCategory("resize");
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
