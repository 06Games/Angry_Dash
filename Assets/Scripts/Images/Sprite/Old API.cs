using LibAPNG;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using _Rect = UnityEngine.Rect;

namespace AngryDash.Image
{
    internal class OLD_API : MonoBehaviour
    {
        /// <summary>
        /// Request an animation (or a sprite)
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="border">Border of the Sprites</param>
        /// <returns></returns>
        public static Sprite_API_Data GetSprites(string filePath, Vector4 border = new Vector4(), bool forcePNG = false)
        {
            Load(filePath, border, forcePNG);
            return Cache.Open("Ressources/textures").Get<Sprite_API_Data>(filePath) ?? new Sprite_API_Data();
        }

        public static void Load(string filePath, Vector4 border = new Vector4(), bool forcePNG = false)
        {
            Cache cache = Cache.Open("Ressources/textures");
            if (cache.ValueExist(filePath)) return;
            if (File.Exists(filePath))
            {
                APNG apng = new APNG(filePath);
                Sprite_API_Data SAD = new Sprite_API_Data();

                List<float> Delay = new List<float>();
                List<Sprite> Frames = new List<Sprite>();

                if (apng.IsSimplePNG | !ConfigAPI.GetBool("video.APNG") | forcePNG) //PNG
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(File.ReadAllBytes(filePath));
                    tex = Tools.Texture2DExtensions.PremultiplyAlpha(tex);
                    tex.Apply();
                    Frames.Add(Sprite.Create(tex, new _Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, border));
                    Frames[0].name = Path.GetFileNameWithoutExtension(filePath);
                }
                else //APNG
                {
                    SAD.Repeat = apng.acTLChunk.NumPlays;

                    APNGFrameInfo info = new APNGFrameInfo(filePath, apng, 0, border);
                    for (int i = 0; i < apng.Frames.Length; i++)
                    {
                        float delay;
                        if (apng.Frames[i].fcTLChunk.DelayNum == 0) delay = 10000;
                        else if (apng.Frames[i].fcTLChunk.DelayDen == 0) delay = apng.Frames[i].fcTLChunk.DelayNum / 100F;
                        else delay = apng.Frames[i].fcTLChunk.DelayNum / (float)apng.Frames[i].fcTLChunk.DelayDen;
                        if (delay == 0) delay = 60;
                        Delay.Add(delay);

                        info.index = i;
                        Frames.Add(GetSprite(info));
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
                Color[] fgColor = frameImg.GetPixels();
                Color[] bgColor = frameTampon.GetPixels((int)offset.x, (int)offset.y, frameImg.width, frameImg.height);
                for (int c = 0; c < fgColor.Length; c++)
                {
                    if (fgColor[c].a == 0) continue;
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
            return Sprite.Create(frameTampon, new _Rect(0, 0, frameTampon.width, frameTampon.height), new Vector2(.5f, .5f),
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
