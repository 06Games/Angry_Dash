using LibAPNG;
using System.Drawing;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Sprite_API
{
    /// <summary>
    /// Contains animation information
    /// </summary>
    [System.Serializable]
    public class Sprite_API_Data
    {
        /// <summary>
        /// Array of all the frames of the annimation
        /// (If the ressource is only a sprite, the sprite will be returned at the index 0)
        /// </summary>
        public Sprite[] Frames;
        /// <summary>
        /// Delay before each frame
        /// </summary>
        public float[] Delay;
        /// <summary>
        /// Number of repetitions of the animation (0 being infinity)
        /// </summary>
        public uint Repeat;

        public Sprite_API_Data()
        {
            Frames = new Sprite[0];
            Delay = new float[0];
            Repeat = 0;
        }
        public Sprite_API_Data(Sprite[] frames, float[] delay, uint repeat)
        {
            Frames = frames;
            Delay = delay;
            Repeat = repeat;
        }
    }

    [System.Serializable]
    public class JSON_PARSE_DATA
    {
        [Header("Textures")]
        public string[] path = new string[4];
        public Vector4[] border = new Vector4[4];
        public int[] type = new int[4];

        [Space(10)]
        [Header("Text Components")]
        public Color32 textColor;
        public UnityEngine.FontStyle textStyle;
        public TextAnchor textAnchor;
        public bool textResize;
        public int textSize; //Only if textResize = false or for compatibility with layout groups
        public int[] textResizeMinAndMax = new int[2];  //Only works if textResize = true
    }


    public class Sprite_API : MonoBehaviour
    {
        /// <summary>
        /// Path to the Ressources
        /// </summary>
        public static string spritesPath(string id)
        {
#if UNITY_EDITOR
            if (!id.Contains("bg") & !string.IsNullOrEmpty(id))
            {
                string fid = id.Replace(" basic", "").Replace(" hover", "").Replace(" pressed", "").Replace(" disabled", "");
                string idPath = Application.dataPath + "/rpID.txt";
                string[] lines = new string[0];
                if (File.Exists(idPath)) lines = File.ReadAllLines(idPath);
                fid = fid.Replace(".png", "").Replace(".json", "");
                if(!string.IsNullOrEmpty(fid)) File.WriteAllLines(idPath, lines.Union(new string[] { fid }));
            }

#endif

            if (ConfigAPI.GetString("ressources.pack") == null)
                ConfigAPI.SetString("ressources.pack", "default");
            string path = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/" + id;
            if (File.Exists(path)) return path;
            else return Application.persistentDataPath + "/Ressources/default/textures/" + id;
        }

        public static JSON_PARSE_DATA Parse(string baseID, FileFormat.JSON.JSON json = null)
        {
            CacheManager.Cache cache = new CacheManager.Cache("Ressources/textures/json");
            if (!cache.ValueExist(baseID))
            {
                if (json == null)
                {
                    json = new FileFormat.JSON.JSON("");
                    string jsonID = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/" + baseID + ".json";
                    if (File.Exists(jsonID)) json = new FileFormat.JSON.JSON(File.ReadAllText(jsonID));
                }
                cache.Set(baseID, LoadParse(baseID, json));
            }
            return cache.Get<JSON_PARSE_DATA>(baseID);
        }
        public static JSON_PARSE_DATA LoadParse(string baseID, FileFormat.JSON.JSON json)
        {
            JSON_PARSE_DATA data = new JSON_PARSE_DATA();

            //Textures
            if (true)
            {
                FileFormat.JSON.Category category = json.GetCategory("textures");

                string[] paramNames = new string[] { "basic", "hover", "pressed", "disabled" };
                data.path = new string[paramNames.Length];
                data.border = new Vector4[paramNames.Length];
                data.type = new int[paramNames.Length];
                for (int i = 0; i < paramNames.Length; i++)
                {
                    FileFormat.JSON.Category paramCategory = category.GetCategory(paramNames[i]);

                    data.path[i] = spritesPath(baseID + " " + paramNames[i] + ".png");
                    Vector4 border = new Vector4();
                    if (paramCategory.ContainsValues)
                    {
                        //Border
                        FileFormat.JSON.Category borderCategory = paramCategory.GetCategory("border");
                        if (borderCategory.ContainsValues)
                        {
                            if (borderCategory.ValueExist("left")) border.x = borderCategory.Value<float>("left");
                            if (borderCategory.ValueExist("right")) border.z = borderCategory.Value<float>("right");
                            if (borderCategory.ValueExist("top")) border.w = borderCategory.Value<float>("top");
                            if (borderCategory.ValueExist("bottom")) border.y = borderCategory.Value<float>("bottom");
                        }

                        //Path
                        if (paramCategory.ValueExist("path"))
                            data.path[i] = new FileInfo(spritesPath(baseID + ".json")).Directory.FullName +
                                "/" + paramCategory.Value<string>("path");

                        if (paramCategory.ValueExist("type"))
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
                FileFormat.JSON.Category category = json.GetCategory("text");
                //Color
                Color32 textColor = new Color32(255, 255, 255, 255);
                if (category.ValueExist("color")) HexColorField.HexToColor(category.Value<string>("color"), out textColor);
                data.textColor = textColor;

                //Font Style
                if (category.ValueExist("fontStyle"))
                {
                    string value = category.Value<string>("fontStyle");
                    if (value == "Normal") data.textStyle = UnityEngine.FontStyle.Normal;
                    else if (value == "Bold") data.textStyle = UnityEngine.FontStyle.Bold;
                    else if (value == "Italic") data.textStyle = UnityEngine.FontStyle.Italic;
                    else if (value == "BoldAndItalic") data.textStyle = UnityEngine.FontStyle.BoldAndItalic;
                }
                else data.textStyle = UnityEngine.FontStyle.Normal;

                //Font Alignment
                FileFormat.JSON.Category fontAlignment = category.GetCategory("fontAlignment");
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

                    data.textAnchor = (TextAnchor)((vertical * 3) + horizontal);
                }
                else data.textAnchor = TextAnchor.MiddleLeft;

                //Font Size
                FileFormat.JSON.Category fontSize = category.GetCategory("resize");
                if (fontSize.ValueExist("minSize") & fontSize.ValueExist("maxSize")) data.textResize = true;
                else { data.textResize = false; data.textSize = 14; }
                if (fontSize.ValueExist("minSize")) data.textResizeMinAndMax[0] = fontSize.Value<int>("minSize");
                if (fontSize.ValueExist("maxSize")) data.textResizeMinAndMax[1] = fontSize.Value<int>("maxSize");
            }

            return data;
        }

        /// <summary>
        /// Request an animation (or a sprite)
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="border">Border of the Sprites</param>
        /// <returns></returns>
        public static Sprite_API_Data GetSprites(string filePath, Vector4 border = new Vector4())
        {
            Load(filePath, border);
            return new CacheManager.Cache("Ressources/textures").Get<Sprite_API_Data>(filePath);
        }

        public static void Load(string filePath, Vector4 border = new Vector4())
        {
            CacheManager.Cache cache = new CacheManager.Cache("Ressources/textures");
            if (cache.ValueExist(filePath)) return;
            if (File.Exists(filePath))
            {
                APNG apng = new APNG(filePath);
                Sprite_API_Data SAD = new Sprite_API_Data();

                float[] Delay = new float[apng.Frames.Length];
                Sprite[] Frames = new Sprite[apng.Frames.Length];

                if (apng.IsSimplePNG) //PNG
                {
                    Frames = new Sprite[1];
                    Delay = new float[1] { 0 };
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(File.ReadAllBytes(filePath));
                    Frames[0] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f),
                        100, 0, SpriteMeshType.FullRect, border);
                }
                else //APNG
                {
                    Frames = new Sprite[apng.Frames.Length];
                    Delay = new float[apng.Frames.Length];
                    SAD.Repeat = apng.acTLChunk.NumPlays;

                    APNGFrameInfo info = new APNGFrameInfo(filePath, apng, 0, border);
                    for (int i = 0; i < apng.Frames.Length; i++)
                    {
                        if (apng.Frames[i].fcTLChunk.DelayNum == 0) Delay[i] = 10000;
                        else if (apng.Frames[i].fcTLChunk.DelayDen == 0) Delay[i] = apng.Frames[i].fcTLChunk.DelayNum / 100F;
                        else Delay[i] = apng.Frames[i].fcTLChunk.DelayNum / (float)apng.Frames[i].fcTLChunk.DelayDen;
                        if (Delay[i] == 0) Delay[i] = 60;

                        info.index = i;
                        Frames[i] = GetSprite(info);
                    }

                    for (int i = 0; i < info.errors.Length; i++)
                        BaseControl.LogNewMassage(info.errors[i]);
                }

                SAD.Frames = Frames;
                SAD.Delay = Delay;
                cache.Set(filePath, SAD);
            }
        }

        public class APNGFrameInfo
        {
            public string id { get; set; }
            public APNG apng { get; set; }
            public Vector4 border { get; set; }
            public int index { get; set; }
            public Texture2D buffer { get; set; }
            public DisposeOps dispose { get; set; }
            public string[] errors { get; set; }

            public APNGFrameInfo(string identifier, APNG png, int i = 0, Vector4 Border = new Vector4())
            {
                id = identifier;
                apng = png;
                border = Border;
                index = i;
                buffer = CreateTransparent(png.IHDRChunk.Width, png.IHDRChunk.Height);
                dispose = apng.DefaultImage.fcTLChunk.DisposeOp;
                errors = new string[0];
            }
        }

        static Sprite GetSprite(APNGFrameInfo info)
        {
            Frame frame = info.apng.Frames[info.index];

            Texture2D text = new Texture2D(1, 1);
            text.LoadImage(frame.GetStream().ToArray());

            info.buffer.SetPixels((int)frame.fcTLChunk.XOffset, info.buffer.height - (int)frame.fcTLChunk.YOffset - text.height,
                text.width, text.height, text.GetPixels(0, 0, text.width, text.height));

            Texture2D tex = info.buffer;
            tex.Apply();

            info.dispose = frame.fcTLChunk.DisposeOp;
            TamponCleaner(info);

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f),
                        100, 0, SpriteMeshType.FullRect, info.border);
        }

        static void TamponCleaner(APNGFrameInfo info)
        {
            if (info.index == 0 & info.dispose == DisposeOps.APNGDisposeOpPrevious) info.dispose = DisposeOps.APNGDisposeOpBackground;

            if (info.dispose == DisposeOps.APNGDisposeOpPrevious)
            {
                Texture2D text = new Texture2D(1, 1);
                text.LoadImage(info.apng.Frames[info.index - 1].GetStream().ToArray());

                info.buffer = CreateTransparent(info.apng.IHDRChunk.Width, info.apng.IHDRChunk.Height);
                Frame frame = info.apng.Frames[info.index - 1];
                info.buffer.SetPixels((int)frame.fcTLChunk.XOffset, info.buffer.height - (int)frame.fcTLChunk.YOffset - text.height, text.width, text.height,
                    text.GetPixels(0, 0, text.width, text.height));
            }
            else if (info.dispose == DisposeOps.APNGDisposeOpBackground)
                info.buffer = CreateTransparent(info.apng.IHDRChunk.Width, info.apng.IHDRChunk.Height);
            else if (info.dispose == DisposeOps.APNGDisposeOpNone)
                info.errors = info.errors.Union(new string[] { "The texture at \"" + info.id + "\" contains frames with the dispose value set to None, the game does not support it yet !" }).ToArray();
        }

        static Texture2D CreateTransparent(int width, int height)
        {
            Texture2D texture_ = new Texture2D(width, height);

            Color32 resetColor = UnityEngine.Color.white;
            Color32[] resetColorArray = texture_.GetPixels32();
            for (int i = 0; i < resetColorArray.Length; i++)
                resetColorArray[i] = resetColor;

            texture_.SetPixels32(resetColorArray);
            texture_.Apply();
            return texture_;
        }
    }
}
