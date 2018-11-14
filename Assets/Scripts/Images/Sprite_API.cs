using LibAPNG;
using System.Drawing;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Sprite_API
{
    public class Sprite_API_Data
    {
        public Sprite[] Frames;
        public float[] Delay;
        public uint Repeat;
    }

    public class Sprite_API : MonoBehaviour
    {        
        public static string spritesPath = Application.persistentDataPath + "/Ressources/default/textures/";

        public static Sprite_API_Data GetSprites(string id, bool isPath = false)
        {
            string filePath = id;
            if(!isPath) filePath = spritesPath + id + ".png";

            if (File.Exists(filePath))
            {
                APNG apng = new APNG(filePath);
                float[] Delay = new float[apng.Frames.Length];
                Sprite[] Frames = new Sprite[apng.Frames.Length];

                /*Bitmap[] frames = UpdateUI(apng);
                for (int i = 0; i < frames.Length; i++)
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(ToByteArray(frames[i], System.Drawing.Imaging.ImageFormat.Png));
                    tex.Apply();
                    Frames[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));

                    if (apng.Frames[i].fcTLChunk.DelayNum == 0) Delay[i] = 10000;
                    else if (apng.Frames[i].fcTLChunk.DelayDen == 0) Delay[i] = 100F / apng.Frames[i].fcTLChunk.DelayNum;
                    else Delay[i] = (float)apng.Frames[i].fcTLChunk.DelayDen / apng.Frames[i].fcTLChunk.DelayNum;
                    if (Delay[i] == 0) Delay[i] = 60;
                }*/

                if (apng.IsSimplePNG) //PNG
                {
                    Frames = new Sprite[1];
                    Delay = new float[1] { 0 };
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(File.ReadAllBytes(filePath));
                    Frames[0] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                }
                else //APNG
                {
                    Frames = new Sprite[apng.Frames.Length];
                    Delay = new float[apng.Frames.Length];

                    APNGFrameInfo info = new APNGFrameInfo(id, apng);
                    for (int i = 0; i < apng.Frames.Length; i++)
                    {
                        if (apng.Frames[i].fcTLChunk.DelayNum == 0) Delay[i] = 10000;
                        else if (apng.Frames[i].fcTLChunk.DelayDen == 0) Delay[i] = 100F / apng.Frames[i].fcTLChunk.DelayNum;
                        else Delay[i] = (float)apng.Frames[i].fcTLChunk.DelayDen / apng.Frames[i].fcTLChunk.DelayNum;
                        if (Delay[i] == 0) Delay[i] = 60;

                        info.index = i;
                        Frames[i] = GetSprite(info);
                    }

                    for (int i = 0; i < info.errors.Length; i++)
                        BaseControl.LogNewMassage(info.errors[i]);
                }

                Sprite_API_Data SAD = new Sprite_API_Data();
                SAD.Frames = Frames;
                SAD.Delay = Delay;
                SAD.Repeat = apng.acTLChunk.NumPlays;
                return SAD;
            }
            else return null;
        }

        public class APNGFrameInfo
        {
            public string id { get; set; }
            public APNG apng { get; set; }
            public int index { get; set; }
            public Texture2D buffer { get; set; }
            public DisposeOps dispose { get; set; }
            public string[] errors { get; set; }

            public APNGFrameInfo(string identifier, APNG png, int i = 0)
            {
                id = identifier;
                apng = png;
                index = i;
                buffer = CreateTransparent(png.IHDRChunk.Width, png.IHDRChunk.Height);
                dispose = apng.DefaultImage.fcTLChunk.DisposeOp;
                errors = new string[0];
            }
        }
        
        public static Sprite GetSprite(APNGFrameInfo info)
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

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
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
                info.errors = info.errors.Union(new string[] { "The texture with id \"" + info.id + "\" contains frames with the dispose value set to None, the game does not support it yet !" }).ToArray();
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
