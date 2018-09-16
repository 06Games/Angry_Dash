using LibAPNG;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Sprite_API
{
    public class Sprite_API_Data
    {
        public Sprite[] Frames;
        public float FramePerSeconds;
    }

    public class Sprite_API : MonoBehaviour
    {

        static string spritesPath { get { return Application.persistentDataPath + "/Ressources/default/"; } }

        public static Sprite_API_Data GetSprites(string id, bool isPath = false)
        {
            string filePath = spritesPath + id + ".png";
            if (File.Exists(filePath))
            {
                APNG apng = new APNG(filePath);
                Sprite[] Frames;
                float FramesPerSeconds = 0;

                if (apng.IsSimplePNG) //PNG
                {
                    Frames = new Sprite[1];
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(File.ReadAllBytes(filePath));
                    Frames[0] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                }
                else //APNG
                {
                    Frames = new Sprite[apng.Frames.Length];
                    if (apng.DefaultImage.fcTLChunk.DelayNum == 0) FramesPerSeconds = 10000;
                    else if (apng.DefaultImage.fcTLChunk.DelayDen == 0) FramesPerSeconds = 100F / apng.DefaultImage.fcTLChunk.DelayNum;
                    else FramesPerSeconds = (float)apng.DefaultImage.fcTLChunk.DelayDen / apng.DefaultImage.fcTLChunk.DelayNum;
                    if (FramesPerSeconds == 0) FramesPerSeconds = 60;

                    for (int i = 0; i < apng.Frames.Length; i++)
                        Frames[i] = GetSprite(apng, i);
                }

                Sprite_API_Data SAD = new Sprite_API_Data();
                SAD.Frames = Frames;
                SAD.FramePerSeconds = FramesPerSeconds;
                return SAD;
            }
            else return null;
        }
        public static Sprite GetSprite(APNG apng, int index)
        {
            Frame frame = apng.Frames[index];

            Texture2D text = new Texture2D(1, 1);
            text.LoadImage(frame.GetStream().ToArray());

            Texture2D tex = CreateTransparent((int)apng.DefaultImage.fcTLChunk.Width, (int)apng.DefaultImage.fcTLChunk.Height);


            tex.SetPixels((int)frame.fcTLChunk.XOffset, tex.height - (int)frame.fcTLChunk.YOffset - text.height, text.width, text.height,
                text.GetPixels(0, 0, text.width, text.height));
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
        }


        static Texture2D CreateTransparent(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.ARGB32, false);
            UnityEngine.Color fillColor = UnityEngine.Color.clear;
            UnityEngine.Color[] fillPixels = new UnityEngine.Color[tex.width * tex.height];
            for (int v = 0; v < fillPixels.Length; v++)
            {
                fillPixels[v] = fillColor;
            }
            tex.SetPixels(fillPixels);
            tex.Apply();

            return tex;
        }
    }
}
