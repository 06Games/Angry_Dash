using Hjg.Pngcs;
using LibAPNG;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AngryDash.Image
{
    public partial class Sprite_API : MonoBehaviour
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
            Sprite_API_Data SAD = null;
            await Task.Run(() => LoadAsync(filePath, border, (sad) => SAD = sad, forcePNG));
            return SAD;
        }
        public static Sprite_API_Data GetSprites(string filePath) { return new CacheManager.Cache("Ressources/textures").Get<Sprite_API_Data>(filePath); }

        internal class APNGFrameInfo
        {
            public string id { get; set; }
            public Frame[] frames { get; set; }
            public Vector4 border { get; set; } = new Vector4();
            public Texture buffer { get; set; }
            public DisposeOps dispose { get; set; }
            public string[] errors { get; set; } = new string[0];

            public APNGFrameInfo(string identifier, APNG apng)
            {
                id = identifier;
                buffer = new Texture(apng.IHDRChunk.Width, apng.IHDRChunk.Height, (Texture.ColorType)apng.IHDRChunk.ColorType);
                if (apng.DefaultImageIsAnimated) dispose = apng.DefaultImage.fcTLChunk.DisposeOp;
            }
        }

        public static void LoadAsync(string filePath, Vector4 border = new Vector4(), System.Action<Sprite_API_Data> callback = null, bool forcePNG = false)
        {
            UnityThread.executeCoroutine(LoadC());
            System.Collections.IEnumerator LoadC()
            {
                CacheManager.Cache cache = new CacheManager.Cache("Ressources/textures");
                Sprite_API_Data SAD = new Sprite_API_Data();
                if (!cache.ValueExist(filePath) && File.Exists(filePath))
                {
                    APNG apng = new APNG(filePath);
                    APNGFrameInfo info = new APNGFrameInfo(filePath, apng) { border = border };
                    Logging.Log("Loading: " + info.id);

                    bool png = apng.IsSimplePNG | !ConfigAPI.GetBool("video.APNG") | forcePNG;
                    if (png) info.frames = new Frame[] { apng.DefaultImage }; //PNG
                    else //APNG
                    {
                        info.frames = apng.Frames;
                        SAD.Repeat = apng.acTLChunk.NumPlays;
                    }

                    for (int i = 0; i < info.frames.Length; i++)
                    {
                        float delay = 0;
                        if (png) delay = 0;
                        else if (info.frames[i].fcTLChunk.DelayNum == 0) delay = 10000;
                        else
                        {
                            if (info.frames[i].fcTLChunk.DelayDen == 0) delay = info.frames[i].fcTLChunk.DelayNum / 100F;
                            else delay = info.frames[i].fcTLChunk.DelayNum / (float)info.frames[i].fcTLChunk.DelayDen;
                            if (delay == 0) delay = 60;
                        }
                        SAD.Delay.Add(delay);

                        bool done = false;
                        GetSprite(info, i, (s) =>
                        {
                            if (s != null) s.name = Path.GetFileNameWithoutExtension(filePath) + (png ? "": " n° " + i);
                            SAD.Frames.Add(s);
                            done = true;
                        });
                        yield return new WaitUntil(() => done);
                    }

                    foreach (var error in info.errors) Logging.Log(error, LogType.Error);
                    cache.Set(filePath, SAD);
                }
                else SAD = cache.Get<Sprite_API_Data>(filePath) ?? SAD;
                callback?.Invoke(SAD);
            }
        }

        static void GetSprite(APNGFrameInfo info, int index, System.Action<Sprite> callback)
        {
            Texture frameTampon = info.buffer;
            Task.Run(() =>
            {
                Frame frame = info.frames[index];
                var rect = new Rect(
                    frame.fcTLChunk != null ? new uint[] { frame.fcTLChunk.XOffset, info.buffer.height - frame.fcTLChunk.YOffset - frame.fcTLChunk.Height } : new uint[] { 0, 0 },
                    frame.fcTLChunk != null ? new uint[] { frame.fcTLChunk.Width, frame.fcTLChunk.Height } : new uint[] { frameTampon.width, frameTampon.height }
                );
                frameTampon.SetPixels(rect, ProcessSprite(info, frame, frameTampon.GetPixels(rect)));
            }).ContinueWith((t) =>
            {
                if (t.Exception != null) Debug.LogError(info.id + "\n" + t.Exception);
                UnityThread.executeInUpdate(() =>
                {
                    try { 
                        var sp = frameTampon.ToSprite(info.border);
                        if (info.frames[index].fcTLChunk != null) TamponCleaner(info, index, frameTampon);
                        callback(sp);
                    }
                    catch (System.Exception e) { Debug.LogError(info.id + "\n" + e.Message); callback(null); }
                });
            });
        }
        static void TamponCleaner(APNGFrameInfo info, int index, Texture frameTampon)
        {
            if (index == 0 & info.dispose == DisposeOps.APNGDisposeOpPrevious) //Previous in the first frame
                info.dispose = DisposeOps.APNGDisposeOpBackground; //is treated as Background

            if (info.dispose == DisposeOps.APNGDisposeOpPrevious) { } //don't apply anything
            else if (info.dispose == DisposeOps.APNGDisposeOpBackground) //reset the buffer
                info.buffer.Clear();
            else if (info.dispose == DisposeOps.APNGDisposeOpNone) //set the frame to the buffer
                info.buffer = frameTampon;
        }

        static List<int> ProcessSprite(APNGFrameInfo info, Frame frame, List<int> bgColors)
        {
            PngReader png = new PngReader(new MemoryStream(frame.GetStream().ToArray()));
            var blendOp = frame.fcTLChunk != null ? frame.fcTLChunk.BlendOp : BlendOps.APNGBlendOpSource;
            int colorNb = png.ImgInfo.Channels;
            if (blendOp == BlendOps.APNGBlendOpSource) bgColors.Clear();

            var PLTE = frame.OtherChunks.FirstOrDefault(c => c.ChunkType == "PLTE")?.ChunkData.Select(c => (int)c).ToList();

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for (int y = 0; y < png.ImgInfo.Rows; y++)
            {
                var row = png.ReadRow(y).Scanline;

                if (blendOp == BlendOps.APNGBlendOpSource && info.buffer.colorType != Texture.ColorType.palette) bgColors.AddRange(row);
                else
                {
                    int bgIndex = y * info.buffer.colorBand;
                    for (int x = 0, xIndex = 0; xIndex < row.Length; xIndex += colorNb, x++)
                    {
                        if (blendOp == BlendOps.APNGBlendOpOver)
                        {
                            int a = colorNb == 4 ? row[xIndex + 3] : 255;
                            if (a == 255) bgColors[bgIndex + x] = row[xIndex];
                            else if (a > 0) for (int i = 0; i < 3; i++) bgColors[bgIndex + i] = a * row[xIndex + i] + (1 - a) * row[xIndex + i];
                        }
                        else if (blendOp == BlendOps.APNGBlendOpSource & PLTE != null)
                        {
                            if (xIndex >= row.Length) Debug.LogError(xIndex + " / " + row.Length + " - " + colorNb + "\n" + y + " - " + info.buffer.colorBand);
                            else if (PLTE.Count > row[xIndex] * 3 + 2) bgColors.AddRange(PLTE.GetRange(row[xIndex] * 3, 3));
                        }
                    }
                }

                if (info.buffer.colorType == Texture.ColorType.palette && png.ImgInfo.Cols > row.Length) bgColors.AddRange(new int[(png.ImgInfo.Cols - row.Length) * info.buffer.colorBand]);
            }

            Logging.Log("Done: " + info.id + "\n" + png.ImgInfo.Rows + " rows in " + sw.Elapsed + "\nBlend Op: " + blendOp);
            png.End();
            sw.Stop();

            info.dispose = frame.fcTLChunk != null ? frame.fcTLChunk.DisposeOp : DisposeOps.APNGDisposeOpNone;
            return bgColors;
        }
    }
}
