using LibAPNG;
using System.IO;
using Tools;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Sprite_API
{
    public struct Data
    {
        public System.IntPtr path;
        public Vector4 border;
        public int forcePNG;
    }

    public struct Sprite_API_Job : IJobParallelFor
    {
        public NativeArray<Data> data;
        public int state;

        public void Execute(int index)
        {
            state = index;
            string filePath = System.Runtime.InteropServices.Marshal.PtrToStringBSTR(data[index].path);

            CacheManager.Cache cache = new CacheManager.Cache("Ressources/textures");
            if (cache.ValueExist(filePath)) return;
            if (File.Exists(filePath))
            {
                APNG apng = new APNG(filePath);

                float[] Delay = new float[1];
                uint repeat = 0;
                Color32[][] Frames = new Color32[1][];

                if (apng.IsSimplePNG | !ConfigAPI.GetBool("video.APNG") | data[index].forcePNG == 1) //PNG
                {
                    Delay = new float[1] { 0 };
                    System.Drawing.Bitmap img = LoadBitmap(File.ReadAllBytes(filePath));
                    NativeArray<Color32> tex = LoadImage(img);

                    Frames = new Color32[1][] { Texture2DExtensions.PremultiplyAlpha(tex).ToArray() };
                }
                else //APNG
                {
                    Frames = new Color32[apng.Frames.Length][];
                    Delay = new float[apng.Frames.Length];
                    repeat = apng.acTLChunk.NumPlays;

                    APNGFrameInfo info = new APNGFrameInfo(filePath, apng, 0, data[index].border);
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
                        Logging.Log(info.errors[i], LogType.Error);
                }

                Vector4 Border = data[index].border;
                UnityThread.executeInUpdate(() =>
                {
                    Sprite[] frames = new Sprite[Frames.Length];
                    for (int i = 0; i < Frames.Length; i++)
                    {
                        Texture2D tex = new Texture2D(apng.IHDRChunk.Width, apng.IHDRChunk.Height);
                        tex.SetPixels32(Frames[i]);
                        tex.Apply();
                        frames[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, Border);
                        frames[i].name = Path.GetFileNameWithoutExtension(filePath) + (Frames.Length > 1 ? " n°" + i : "");
                    }

                    new CacheManager.Cache("Ressources/textures").Set(filePath, new Sprite_API_Data() { Frames = frames, Delay = Delay, Repeat = repeat });
                });
            }
        }

        public class APNGFrameInfo
        {
            public string id { get; set; }
            public APNG apng { get; set; }
            public Vector4 border { get; set; }
            public int index { get; set; }
            public NativeArray<Color32> buffer { get; set; }
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

        static Color32[] GetSprite(APNGFrameInfo info)
        {
            Frame frame = info.apng.Frames[info.index];

            System.Drawing.Bitmap img = LoadBitmap(frame.GetStream().ToArray());
            NativeArray<Color32> frameImg = LoadImage(img);

            NativeArray <Color32> frameTampon = info.buffer;
            Vector2 offset = new Vector2(frame.fcTLChunk.XOffset, info.apng.IHDRChunk.Height - frame.fcTLChunk.YOffset - img.Height);
            if (frame.fcTLChunk.BlendOp == BlendOps.APNGBlendOpOver)
            {
                Color32[] fgColor = frameImg.ToArray();
                Color32[] bgColor = GetPixels(
                    frameTampon,
                    new Vector2Int(info.apng.IHDRChunk.Width, info.apng.IHDRChunk.Height),
                    (int)offset.x,
                    (int)offset.y,
                    img.Width,
                    img.Height);
                for (int c = 0; c < fgColor.Length; c++)
                {
                    if (fgColor[c].a == 0) { /* Do nothing */ }
                    else if (fgColor[c].a == 255) bgColor[c] = fgColor[c];
                    else
                    {
                        Color color = bgColor[c];
                        Color fColor = fgColor[c];
                        for (int i = 0; i < 3; i++) color[i] = fgColor[c].a * fColor[i] + (1 - fColor.a) * color[i];
                        bgColor[c] = color;
                    }
                }
                SetPixels(
                    frameTampon,
                    new Vector2Int(info.apng.IHDRChunk.Width, info.apng.IHDRChunk.Height), (int)offset.x, (int)offset.y, img.Width, img.Height, bgColor);
            }
            else if (frame.fcTLChunk.BlendOp == BlendOps.APNGBlendOpSource)
            {
                SetPixels(
                    frameTampon,
                    new Vector2Int(info.apng.IHDRChunk.Width, info.apng.IHDRChunk.Height), (int)offset.x, (int)offset.y, img.Width, img.Height, //Image position
                    frameImg.ToArray()); //Copy image
            }

            info.dispose = frame.fcTLChunk.DisposeOp;
            TamponCleaner(info, frameTampon);

            frameImg.Dispose();
            Color32[] array = frameTampon.ToArray();
            frameTampon.Dispose();
            return array;
        }


        public static System.Drawing.Bitmap LoadBitmap(byte[] blob)
        {
            using (MemoryStream mStream = new MemoryStream())
            {
                mStream.Write(blob, 0, blob.Length);
                mStream.Seek(0, SeekOrigin.Begin);

                System.Drawing.Bitmap bm = new System.Drawing.Bitmap(mStream);
                return bm;
            }
        }
        static NativeArray<Color32> LoadImage(System.Drawing.Bitmap img)
        {
            System.Collections.Generic.List<Color32> colors = new System.Collections.Generic.List<Color32>();
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    System.Drawing.Color pixel = img.GetPixel(i, j);
                    colors.Add(new Color32(pixel.R, pixel.G, pixel.B, pixel.A));
                }
            }
            return new NativeArray<Color32>(colors.ToArray(), Allocator.Temp);
        }

        static Color32[] GetPixels(NativeArray<Color32> colors, Vector2Int size, int xOffset, int yOffset, int width, int height)
        {
            int startIndex = xOffset * size.y + yOffset;
            System.Collections.Generic.List<Color32> color = new System.Collections.Generic.List<Color32>();
            var colorArray = colors.ToArray();
            for (int i = 0; i < width; i++)
            {
                startIndex += height;
                color.AddRange(colorArray.Get(startIndex, startIndex + height));
            }
            return color.ToArray();
        }

        static void SetPixels(NativeArray<Color32> colors, Vector2Int size, int xOffset, int yOffset, int width, int height, Color[] setColors)
        {
            int startIndex = xOffset * size.y + yOffset;
            int index = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++) { colors[startIndex + y] = setColors[index]; index++; }
                startIndex += height;
            }
        }
        static void SetPixels(NativeArray<Color32> colors, Vector2Int size, int xOffset, int yOffset, int width, int height, Color32[] setColors)
        {
            int startIndex = xOffset * size.y + yOffset;
            int index = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++) { colors[startIndex + y] = setColors[index]; index++; }
                startIndex += height;
            }
        }

        static void TamponCleaner(APNGFrameInfo info, NativeArray<Color32> frameTampon)
        {
            if (info.index == 0 & info.dispose == DisposeOps.APNGDisposeOpPrevious) //Previous in the first frame
                info.dispose = DisposeOps.APNGDisposeOpBackground; //is treated as Background

            if (info.dispose == DisposeOps.APNGDisposeOpPrevious) { } //don't apply anything
            else if (info.dispose == DisposeOps.APNGDisposeOpBackground) //reset the buffer
                info.buffer = CreateTransparent(info.apng.IHDRChunk.Width, info.apng.IHDRChunk.Height);
            else if (info.dispose == DisposeOps.APNGDisposeOpNone) //set the frame to the buffer
            {
                info.buffer = frameTampon;
            }
        }

        static NativeArray<Color32> CreateTransparent(int width, int height)
        {
            Color32 resetColor = new Color32(0, 0, 0, 0);
            NativeArray<Color32> resetColorArray = new NativeArray<Color32>(width * height, Allocator.Temp);
            for (int i = 0; i < resetColorArray.Length; i++) resetColorArray[i] = resetColor;
            return resetColorArray;
        }
    }
}
