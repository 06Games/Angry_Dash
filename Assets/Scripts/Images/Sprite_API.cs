using Hjg.Pngcs;
using LibAPNG;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AngryDash.Image
{
    public class Sprite_API : MonoBehaviour
    {
        public static string forceRP { get; set; }

        /// <summary>
        /// Get path to a ressource
        /// </summary>
        /// <param name="id">The id of the ressource</param>
        public static string spritesPath(string id)
        {
#if UNITY_EDITOR
            if (!id.Contains("bg") & !id.Contains("languages/") & !id.Contains("common/") & !id.Contains("startUp/button") & !string.IsNullOrEmpty(id))
            {
                string fid = id.Replace(" basic", "").Replace(" hover", "").Replace(" pressed", "").Replace(" disabled", "");
                string idPath = Application.dataPath + "/rpID.txt";
                string[] lines = new string[0];
                if (File.Exists(idPath)) lines = File.ReadAllLines(idPath);
                fid = fid.Replace(".png", "").Replace(".json", "");
                if (!string.IsNullOrEmpty(fid)) File.WriteAllLines(idPath, lines.Union(new string[] { fid }));
            }

#endif
            if (!string.IsNullOrWhiteSpace(forceRP) && File.Exists(forceRP + "textures/" + id)) return forceRP + "textures/" + id;

            if (ConfigAPI.GetString("ressources.pack") == null) ConfigAPI.SetString("ressources.pack", "default");
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
        public static async Task<Sprite_API_Data> GetSpritesAsync(string filePath, Vector4 border = new Vector4(), bool forcePNG = false)
        {
            await Task.Run(() => LoadAsync(filePath, () => { }, border, forcePNG));
            return new CacheManager.Cache("Ressources/textures").Get<Sprite_API_Data>(filePath);
        }
        public static Sprite_API_Data GetSprites(string filePath, Vector4 border = new Vector4(), bool forcePNG = false)
        {
            Load(filePath, border, forcePNG);
            return new CacheManager.Cache("Ressources/textures").Get<Sprite_API_Data>(filePath);
        }

        [System.Obsolete("Use LoadAsync")] public static void Load(string filePath, Vector4 border = new Vector4(), bool forcePNG = false) { }
        public static void LoadAsync(string filePath, System.Action callback, Vector4 border = new Vector4(), bool forcePNG = false)
        {
            UnityThread.executeCoroutine(LoadC());
            System.Collections.IEnumerator LoadC()
            {
                CacheManager.Cache cache = new CacheManager.Cache("Ressources/textures");
                if (!cache.ValueExist(filePath) && File.Exists(filePath))
                {
                    APNG apng = new APNG(filePath);
                    Sprite_API_Data SAD = new Sprite_API_Data();

                    float[] Delay;
                    Sprite[] Frames;

                    APNGFrameInfo info = new APNGFrameInfo(filePath, apng, 0, border);
                    Logging.Log("Loading: " + info.id);
                    if (apng.IsSimplePNG | !ConfigAPI.GetBool("video.APNG") | forcePNG) //PNG
                    {
                        Frames = new Sprite[1];
                        Delay = new float[1] { 0 };
                        info.frame = apng.DefaultImage;
                        GetSprite(info, (s) =>
                        {
                            Frames[0] = s;
                            Frames[0].name = Path.GetFileNameWithoutExtension(filePath);
                        });
                    }
                    else //APNG
                    {
                        Frames = new Sprite[apng.Frames.Length];
                        Delay = new float[apng.Frames.Length];
                        SAD.Repeat = apng.acTLChunk.NumPlays;

                        for (int i = 0; i < apng.Frames.Length; i++)
                        {
                            if (apng.Frames[i].fcTLChunk.DelayNum == 0) Delay[i] = 10000;
                            else if (apng.Frames[i].fcTLChunk.DelayDen == 0) Delay[i] = apng.Frames[i].fcTLChunk.DelayNum / 100F;
                            else Delay[i] = apng.Frames[i].fcTLChunk.DelayNum / (float)apng.Frames[i].fcTLChunk.DelayDen;
                            if (Delay[i] == 0) Delay[i] = 60;

                            info.index = i;
                            info.frame = apng.Frames[i];
                            GetSprite(info, (s) =>
                            {
                                Frames[i] = s;
                                Frames[i].name = Path.GetFileNameWithoutExtension(filePath);
                            });
                            yield return new WaitWhile(() => Frames[i] == null);
                        }
                    }

                    foreach (var error in info.errors) Logging.Log(error, LogType.Error);
                    SAD.Frames = Frames;
                    SAD.Delay = Delay;
                    cache.Set(filePath, SAD);
                    callback();
                }
                else callback();
            }
        }

        public class APNGFrameInfo
        {
            public string id { get; set; }
            public Frame frame { get; set; }
            public APNG apng { get; set; }
            public Vector4 border { get; set; }
            public int index { get; set; }
            public Texture buffer { get; set; }
            public DisposeOps dispose { get; set; }
            public string[] errors { get; set; }

            public APNGFrameInfo(string identifier, APNG png, int i = 0, Vector4 Border = new Vector4())
            {
                id = identifier;
                apng = png;
                border = Border;
                index = i;
                buffer = new Texture(apng.IHDRChunk.Width, apng.IHDRChunk.Height, (Texture.ColorType)apng.IHDRChunk.ColorType);
                if (png.DefaultImageIsAnimated) dispose = png.DefaultImage.fcTLChunk.DisposeOp;
                errors = new string[0];
            }
        }

        public class Texture
        {
            List<int> pixels { get; set; } = new List<int>();
            public uint width { get; private set; }
            public uint height { get; private set; }
            public int colorBand { get; private set; }
            public enum ColorType { grayscale = 0, grayscaleA = 4, RGB = 2, RGBA = 6, palette = 3 }

            public Texture(int x, int y, ColorType colorBand)
            {
                var background = new int[4];
                if (colorBand == ColorType.grayscale) background = new int[1];
                else if (colorBand == ColorType.grayscaleA) background = new int[2];
                else if (colorBand == ColorType.RGB) background = new int[3];
                else if (colorBand == ColorType.RGBA) background = new int[4];
                else if (colorBand == ColorType.palette) background = new int[3];

                Instanciate(x, y, background);
            }
            public Texture(int x, int y, int[] background = null) { Instanciate(x, y, background ?? new int[4]); }
            void Instanciate(int x, int y, int[] background)
            {
                for (int i = 0; i < x * y; i++) pixels.AddRange(background);

                width = (uint)x;
                height = (uint)y;
                colorBand = background.Length;
            }

            public List<int> GetPixels(long xOffset, long yOffset, long xSize, long ySize)
            {
                if (xOffset == 0 & yOffset == 0 & xSize == width & ySize == height) return pixels;
                else
                {
                    var bounds = WorkOnPixels(xOffset, yOffset, xSize, ySize);
                    return pixels.GetRange(bounds.Item1, bounds.Item2 - bounds.Item1);
                }
            }
            public void SetPixels(long xOffset, long yOffset, long xSize, long ySize, List<int> colors)
            {
                if (pixels.Count == colors.Count) pixels = colors;
                else
                {
                    var bounds = WorkOnPixels(xOffset, yOffset, xSize, ySize);
                    for (int i = bounds.Item1; i < bounds.Item2 & i < colors.Count - bounds.Item1; i++) pixels[i] = colors[i - bounds.Item1];
                }
            }
            (int, int) WorkOnPixels(long xOffset, long yOffset, long xSize, long ySize)
            {
                long minIndex = width * yOffset + xOffset;
                long maxIndex = width * (yOffset + ySize - 1) + (xOffset + xSize);
                return ((int)minIndex * colorBand, (int)maxIndex * colorBand);
            }

            public Sprite ToSprite(Vector4 border)
            {
                var size = new Vector2Int((int)width, (int)height);
                var format = TextureFormat.RGB24;
                if (colorBand == 1) format = TextureFormat.Alpha8;
                else if (colorBand == 3) format = TextureFormat.RGB24;
                else if (colorBand == 4) format = TextureFormat.RGBA32;
                var texture = new Texture2D(size.x, size.y, format, false);

                try { texture.LoadRawTextureData(pixels.Select(i => (byte)i).ToArray()); }
                catch { Debug.LogError("[" + colorBand + " - " + format + "] Espected : " + texture.GetRawTextureData().Length + ", Got : " + pixels.Count); }
                FlipTextureVertically(texture);

                texture.Apply();
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);
            }
        }

        public static void FlipTextureVertically(Texture2D original)
        {
            var originalPixels = original.GetPixels();

            Color[] newPixels = new Color[originalPixels.Length];

            int width = original.width;
            int rows = original.height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    newPixels[x + y * width] = originalPixels[x + (rows - y - 1) * width];
                }
            }

            original.SetPixels(newPixels);
            original.Apply();
        }

        static void GetSprite(APNGFrameInfo info, System.Action<Sprite> callback)
        {
            Texture frameTampon = info.buffer;
            Task.Run(() =>
            {
                Frame frame = info.frame;
                uint[] size = frame.fcTLChunk != null ? new uint[] { frame.fcTLChunk.Width, frame.fcTLChunk.Height } : new uint[] { frameTampon.width, frameTampon.height };
                long[] offset = frame.fcTLChunk != null ? new long[] { frame.fcTLChunk.XOffset, info.buffer.height - frame.fcTLChunk.YOffset - frame.fcTLChunk.Height } : new long[] { 0, 0 };
                var bgColors = frameTampon.GetPixels(offset[0], offset[1], size[0], size[1]);
                frameTampon.SetPixels(offset[0], offset[1], size[0], size[1], ProcessSprite(info, bgColors));
                if (frame.fcTLChunk != null) TamponCleaner(info, frameTampon);
            }).ContinueWith((t) =>
            {
                if (t.Exception != null) Debug.LogError(info.id + "\n" + t.Exception);
                UnityThread.executeInUpdate(() => callback(frameTampon.ToSprite(info.border)));
            });
        }

        static List<int> ProcessSprite(APNGFrameInfo info, List<int> bgColors)
        {
            PngReader png = new PngReader(new MemoryStream(info.frame.GetStream().ToArray()));
            var blendOp = info.frame.fcTLChunk != null ? info.frame.fcTLChunk.BlendOp : BlendOps.APNGBlendOpSource;
            int colorNb = png.ImgInfo.Channels;
            if (blendOp == BlendOps.APNGBlendOpSource) bgColors.Clear();

            var PLTE = info.frame.OtherChunks.FirstOrDefault(c => c.ChunkType == "PLTE")?.ChunkData.ToList();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int y = 0; y < png.ImgInfo.Rows; y++)
            {
                var row = png.ReadRow(y).Scanline;

                if (blendOp == BlendOps.APNGBlendOpSource && info.apng.IHDRChunk.ColorType != 3) bgColors.AddRange(row);
                else
                {
                    int bgIndex = y * info.buffer.colorBand;
                    for (int x = 0, xIndex = 0; x < png.ImgInfo.Cols & xIndex < row.Length; xIndex += colorNb, x++)
                    {
                        if (blendOp == BlendOps.APNGBlendOpOver)
                        {
                            try
                            {
                                int a = colorNb == 4 ? row[xIndex + 3] : 255;
                                if (a == 255) bgColors[bgIndex + x] = row[xIndex];
                                else if (a > 0) for (int i = 0; i < 3; i++) bgColors[bgIndex + i] = a * row[xIndex + i] + (1 - a) * row[xIndex + i];
                            }
                            catch
                            {
                                Debug.LogError(bgColors.Count + " - " + bgIndex + "\n" + png.ImgInfo.Cols + ", " + png.ImgInfo.Rows + "\n" + y);
                            }
                        }
                        else if (blendOp == BlendOps.APNGBlendOpSource & PLTE != null)
                        {
                            if(PLTE.Count > row[xIndex] * 3 + 3) bgColors.AddRange(PLTE.GetRange(row[xIndex] * 3, 3).Select(c => (int)c));
                        }
                    }
                }
            }
            Logging.Log("Done: " + info.id + "\n" + png.ImgInfo.Rows + " rows in " + sw.Elapsed + "\nBlend Op: " + blendOp);
            png.End();
            sw.Stop();

            info.dispose = info.frame.fcTLChunk != null ? info.frame.fcTLChunk.DisposeOp : DisposeOps.APNGDisposeOpNone;
            return bgColors;
        }

        static void TamponCleaner(APNGFrameInfo info, Texture frameTampon)
        {
            if (info.index == 0 & info.dispose == DisposeOps.APNGDisposeOpPrevious) //Previous in the first frame
                info.dispose = DisposeOps.APNGDisposeOpBackground; //is treated as Background

            if (info.dispose == DisposeOps.APNGDisposeOpPrevious) { } //don't apply anything
            else if (info.dispose == DisposeOps.APNGDisposeOpBackground) //reset the buffer
                info.buffer = new Texture(info.apng.IHDRChunk.Width, info.apng.IHDRChunk.Height);
            else if (info.dispose == DisposeOps.APNGDisposeOpNone) //set the frame to the buffer
                info.buffer = frameTampon;
        }
    }
}
