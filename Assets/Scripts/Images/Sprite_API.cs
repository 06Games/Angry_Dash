using LibAPNG;
using System.IO;
using System.Linq;
using UnityEngine;

namespace AngryDash.Image
{
    public class Sprite_API : MonoBehaviour
    {
        /// <summary>
        /// Get path to a ressource
        /// </summary>
        /// <param name="id">The id of the ressource</param>
        public static string spritesPath(string id)
        {
#if UNITY_EDITOR
            if (!id.Contains("bg") & !id.Contains("languages/") & !id.Contains("common/") & !string.IsNullOrEmpty(id))
            {
                string fid = id.Replace(" basic", "").Replace(" hover", "").Replace(" pressed", "").Replace(" disabled", "");
                string idPath = Application.dataPath + "/rpID.txt";
                string[] lines = new string[0];
                if (File.Exists(idPath)) lines = File.ReadAllLines(idPath);
                fid = fid.Replace(".png", "").Replace(".json", "");
                if (!string.IsNullOrEmpty(fid)) File.WriteAllLines(idPath, lines.Union(new string[] { fid }));
            }

#endif

            if (ConfigAPI.GetString("ressources.pack") == null)
                ConfigAPI.SetString("ressources.pack", "default");
            string path = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/" + id;
            if (File.Exists(path)) return path;
            else return Application.persistentDataPath + "/Ressources/default/textures/" + id;
        }

        /// <summary>
        /// Request an animation (or a sprite)
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="border">Border of the Sprites</param>
        /// <returns></returns>
        public static Sprite_API_Data GetSprites(string filePath, Vector4 border = new Vector4(), bool forcePNG = false)
        {
            Load(filePath, border, forcePNG);
            return new CacheManager.Cache("Ressources/textures").Get<Sprite_API_Data>(filePath);
        }

        public static void Load(string filePath, Vector4 border = new Vector4(), bool forcePNG = false)
        {
            CacheManager.Cache cache = new CacheManager.Cache("Ressources/textures");
            if (cache.ValueExist(filePath)) return;
            if (File.Exists(filePath))
            {
                APNG apng = new APNG(filePath);
                Sprite_API_Data SAD = new Sprite_API_Data();

                float[] Delay = new float[apng.Frames.Length];
                Sprite[] Frames = new Sprite[apng.Frames.Length];

                if (apng.IsSimplePNG | !ConfigAPI.GetBool("video.APNG") | forcePNG) //PNG
                {
                    Frames = new Sprite[1];
                    Delay = new float[1] { 0 };
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(File.ReadAllBytes(filePath));
                    tex = Tools.Texture2DExtensions.PremultiplyAlpha(tex);
                    tex.Apply();
                    Frames[0] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f),
                        100, 0, SpriteMeshType.FullRect, border);
                    Frames[0].name = Path.GetFileNameWithoutExtension(filePath);
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
                        Frames[i].name = Path.GetFileNameWithoutExtension(filePath) + " n°" + i;
                    }

                    for (int i = 0; i < info.errors.Length; i++)
                        Logging.Log(info.errors[i], LogType.Error);
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
                if (apng.DefaultImageIsAnimated) dispose = apng.DefaultImage.fcTLChunk.DisposeOp;
                errors = new string[0];
            }
        }

        static Sprite GetSprite(APNGFrameInfo info)
        {
            Frame frame = info.apng.Frames[info.index];

            Texture2D frameImg = new Texture2D(1, 1);
            frameImg.LoadImage(frame.GetStream().ToArray());

            Texture2D frameTampon = info.buffer;
            Vector2 offset = new Vector2(frame.fcTLChunk.XOffset, info.buffer.height - frame.fcTLChunk.YOffset - frameImg.height);
            if (frame.fcTLChunk.BlendOp == BlendOps.APNGBlendOpOver)
            {
                UnityEngine.Color[] fgColor = frameImg.GetPixels();
                UnityEngine.Color[] bgColor = frameTampon.GetPixels((int)offset.x, (int)offset.y, frameImg.width, frameImg.height);
                for (int c = 0; c < fgColor.Length; c++)
                {
                    if (fgColor[c].a == 0) { /* Do nothing */ }
                    else if (fgColor[c].a == 255) bgColor[c] = fgColor[c];
                    else
                    {
                        for (int i = 0; i < 3; i++)
                            bgColor[c][i] = fgColor[c].a * fgColor[c][i] + (1 - fgColor[c].a) * bgColor[c][i];
                    }
                }
                frameTampon.SetPixels((int)offset.x, (int)offset.y, frameImg.width, frameImg.height, bgColor);
            }
            else if (frame.fcTLChunk.BlendOp == BlendOps.APNGBlendOpSource)
            {
                frameTampon.SetPixels((int)offset.x, (int)offset.y, frameImg.width, frameImg.height, //Image position
                frameImg.GetPixels(0, 0, frameImg.width, frameImg.height)); //Copy image
            }

            info.dispose = frame.fcTLChunk.DisposeOp;
            TamponCleaner(info, frameTampon);

            frameTampon.Apply();
            return Sprite.Create(frameTampon, new Rect(0, 0, frameTampon.width, frameTampon.height), new Vector2(.5f, .5f),
                        100, 0, SpriteMeshType.FullRect, info.border);

        }

        static void TamponCleaner(APNGFrameInfo info, Texture2D frameTampon)
        {
            if (info.index == 0 & info.dispose == DisposeOps.APNGDisposeOpPrevious) //Previous in the first frame
                info.dispose = DisposeOps.APNGDisposeOpBackground; //is treated as Background

            if (info.dispose == DisposeOps.APNGDisposeOpPrevious) { } //don't apply anything
            else if (info.dispose == DisposeOps.APNGDisposeOpBackground) //reset the buffer
                info.buffer = CreateTransparent(info.apng.IHDRChunk.Width, info.apng.IHDRChunk.Height);
            else if (info.dispose == DisposeOps.APNGDisposeOpNone) //set the frame to the buffer
            {
                Texture2D texture_ = new Texture2D(info.apng.IHDRChunk.Width, info.apng.IHDRChunk.Height);
                texture_.LoadRawTextureData(frameTampon.GetRawTextureData());
                info.buffer = texture_;
            }
        }

        static Texture2D CreateTransparent(int width, int height)
        {
            Texture2D texture_ = new Texture2D(width, height);

            Color32 resetColor = new Color32(0, 0, 0, 0);
            Color32[] resetColorArray = texture_.GetPixels32();
            for (int i = 0; i < resetColorArray.Length; i++)
                resetColorArray[i] = resetColor;

            texture_.SetPixels32(resetColorArray);
            return texture_;
        }
    }
}
