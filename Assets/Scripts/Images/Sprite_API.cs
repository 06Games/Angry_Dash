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

        public Sprite_API_Data(){
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


    public class Sprite_API : MonoBehaviour
    {        
        /// <summary>
        /// Path to the Ressources
        /// </summary>
        public static string spritesPath
        {
            get
            {
                if (ConfigAPI.GetString("ressources.pack") == null)
                    ConfigAPI.SetString("ressources.pack", "default");
                return Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/";
            }
        }

        /// <summary>
        /// Request an animation (or a sprite)
        /// </summary>
        /// <param name="id">ID of the ressources (can be the full path)</param>
        /// <param name="isPath">Is the ID a full path ?</param>
        /// <returns></returns>
        public static Sprite_API_Data GetSprites(string id, bool isPath = false)
        {
            string filePath = id;
            if(!isPath) filePath = spritesPath + id + ".png";

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
                    Frames[0] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                }
                else //APNG
                {
                    Frames = new Sprite[apng.Frames.Length];
                    Delay = new float[apng.Frames.Length];
                    SAD.Repeat = apng.acTLChunk.NumPlays;

                    APNGFrameInfo info = new APNGFrameInfo(id, apng);
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
                return SAD;
            }
            else return new Sprite_API_Data();
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
