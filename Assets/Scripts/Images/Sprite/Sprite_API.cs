﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hjg.Pngcs;
using LibAPNG;
using Tools;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

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
                var fid = id.Replace(" basic", "").Replace(" hover", "").Replace(" pressed", "").Replace(" disabled", "");
                var idPath = Application.dataPath + "/rpID.txt";
                var lines = new string[0];
                if (File.Exists(idPath)) lines = File.ReadAllLines(idPath);
                fid = fid.Replace(".png", "").Replace(".json", "");
                if (!string.IsNullOrEmpty(fid)) File.WriteAllLines(idPath, lines.Union(new[] { fid }));
            }

#endif
            if (!string.IsNullOrWhiteSpace(forceRP) && File.Exists(forceRP + "textures/" + id)) return forceRP + "textures/" + id;

            if (ConfigAPI.GetString("ressources.pack") == null) ConfigAPI.SetString("ressources.pack", "default");
            var path = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/" + id;
            if (File.Exists(path)) return path;
            return Application.persistentDataPath + "/Ressources/default/textures/" + id;
        }

        /// <summary>
        /// Request an animation (or a sprite)
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <param name="border">Border of the Sprites</param>
        /// <returns></returns>
        public static async Task<Sprite_API_Data> GetSpritesAsync(string filePath, Vector4 border = new Vector4(), bool forcePNG = false)
        {
            if (ConfigAPI.GetBool("ressources.disable")) return null;
            Sprite_API_Data SAD = null;
            await Task.Run(() => LoadAsync(filePath, border, sad => SAD = sad, forcePNG));
            return SAD;
        }
        public static Sprite_API_Data GetSprites(string filePath) { return Cache.Open("Ressources/textures").Get<Sprite_API_Data>(filePath); }

        internal class APNGFrameInfo
        {
            public string id { get; set; }
            public bool animated { get; set; } = true;

            public Frame[] frames { get; set; }
            public Vector4 border { get; set; }
            public Texture buffer { get; set; }
            public DisposeOps dispose { get; set; }

            public string[] errors { get; set; } = new string[0];

            public APNGFrameInfo(string identifier, APNG apng)
            {
                id = identifier;
                buffer = new Texture(apng.IHDRChunk.Width, apng.IHDRChunk.Height, (Texture.ColorType)apng.IHDRChunk.ColorType);
                if (apng.DefaultImageIsAnimated) dispose = apng.DefaultImage.fcTLChunk.DisposeOp;
                else animated = false;
            }
            public override string ToString()
            {
                return $"{id}:\n{frames.Length} frames\nBorder: {border}\nThe buffer is {buffer.GetPixels().Count} long\n" +
                    $"The dispose op is equal to {dispose.ToString().Remove(0, "APNGDisposeOp".Length)}\n\n<b>Errors:</b>\n{string.Join("\n", errors)}";
            }
        }

        public static void LoadAsync(string filePath, Vector4 border = new Vector4(), Action<Sprite_API_Data> callback = null, bool forcePNG = false)
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                var cache = Cache.Open("Ressources/textures");


                var SAD = new Sprite_API_Data();

                var Delay = new List<float>();
                var Frames = new List<Sprite>();

                Frames.Add(Resources.Load<Sprite>(filePath.Replace(Application.persistentDataPath + "/Ressources/default/", "Default/").Replace(".png", "")));

                SAD.Frames = Frames;
                SAD.Delay = Delay;
                cache.Set(filePath, SAD);
                callback.Invoke(SAD);
                return;
            }
#endif
                if (ConfigAPI.GetBool("video.experimentalLoading"))
#if UNITY_EDITOR
                if (!EditorApplication.isPlaying) FindObjectOfType<MonoBehaviour>().StartCoroutine(NEW_API());
                else
#endif
                    UnityThread.executeCoroutine(NEW_API());
            else callback(OLD_API.GetSprites(filePath, border, forcePNG));

            IEnumerator NEW_API()
            {
                var cache = Cache.Open("Ressources/textures");
                var SAD = new Sprite_API_Data();
                if (!cache.ValueExist(filePath) && File.Exists(filePath))
                {
                    var apng = new APNG(filePath);
                    var png = apng.IsSimplePNG | !ConfigAPI.GetBool("video.APNG") | forcePNG;
                    var info = new APNGFrameInfo(filePath, apng) { border = border, animated = !png };
                    Logging.Log("Loading: " + info.id);

                    if (png) info.frames = new[] { apng.DefaultImage }; //PNG
                    else //APNG
                    {
                        info.frames = apng.Frames;
                        SAD.Repeat = apng.acTLChunk.NumPlays;
                    }

                    for (var i = 0; i < info.frames.Length; i++)
                    {
                        float delay = 0; //in seconds
                        if (png) delay = 0;
                        else if (info.frames[i].fcTLChunk.DelayNum == 0) delay = 10000;
                        else
                        {
                            if (info.frames[i].fcTLChunk.DelayDen == 0) delay = info.frames[i].fcTLChunk.DelayNum / 100F;
                            else delay = info.frames[i].fcTLChunk.DelayNum / (float)info.frames[i].fcTLChunk.DelayDen;
                            if (delay == 0) delay = 0.01F; //Skip frame
                        }
                        SAD.Delay.Add(delay);

                        var done = false;
                        GetSprite(info, i, s =>
                        {
                            if (s != null) s.name = Path.GetFileNameWithoutExtension(filePath) + (png ? "" : " n° " + i);
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

        private static void GetSprite(APNGFrameInfo info, int index, Action<Sprite> callback)
        {
            if (!info.animated)
            {
                var tex = new Texture2D(1, 1);
                tex.LoadImage(File.ReadAllBytes(info.id));
                tex = tex.PremultiplyAlpha();
                tex.Apply();
                callback(Sprite.Create(tex, new UnityEngine.Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, info.border));
                return;
            }

            var frameTampon = info.buffer;
            Task.Run(() =>
            {
                var frame = info.frames[index];
                var rect = new Rect(
                    frame.fcTLChunk != null ? new[] { frame.fcTLChunk.XOffset, info.buffer.height - frame.fcTLChunk.YOffset - frame.fcTLChunk.Height } : new uint[] { 0, 0 },
                    frame.fcTLChunk != null ? new[] { frame.fcTLChunk.Width, frame.fcTLChunk.Height } : new[] { frameTampon.width, frameTampon.height }
                );
                frameTampon.SetPixels(rect, ProcessSprite(info, frame, frameTampon.GetPixels(rect)));
            }).ContinueWith(t =>
            {
                if (t.Exception != null) Debug.LogError(info.id + "\n" + t.Exception);
                UnityThread.executeInUpdate(() =>
                {
                    try
                    {
                        var sp = frameTampon.ToSprite(info.border);
                        if (info.frames[index].fcTLChunk != null) TamponCleaner(info, index, frameTampon);
                        callback(sp);
                    }
                    catch (Exception e) { Debug.LogError(info.id + "\n" + e.Message); callback(null); }
                });
            });
        }

        private static void TamponCleaner(APNGFrameInfo info, int index, Texture frameTampon)
        {
            if (index == 0 & info.dispose == DisposeOps.APNGDisposeOpPrevious) //Previous in the first frame
                info.dispose = DisposeOps.APNGDisposeOpBackground; //is treated as Background

            if (info.dispose == DisposeOps.APNGDisposeOpPrevious) { } //don't apply anything
            else if (info.dispose == DisposeOps.APNGDisposeOpBackground) //reset the buffer
                info.buffer.Clear();
            else if (info.dispose == DisposeOps.APNGDisposeOpNone) //set the frame to the buffer
                info.buffer = frameTampon;
        }

        private static List<int> ProcessSprite(APNGFrameInfo info, Frame frame, List<int> bgColors)
        {
            var png = new PngReader(new MemoryStream(frame.GetStream().ToArray()));
            var blendOp = frame.fcTLChunk != null ? frame.fcTLChunk.BlendOp : BlendOps.APNGBlendOpSource;
            var colorNb = png.ImgInfo.Channels;

            var PLTE = frame.OtherChunks.FirstOrDefault(c => c.ChunkType == "PLTE")?.ChunkData.Select(c => (int)c).ToList();
            if (blendOp == BlendOps.APNGBlendOpSource && info.buffer.colorType != Texture.ColorType.palette) bgColors.Clear();

            var sw = new Stopwatch();
            sw.Start();
            for (var y = 0; y < png.ImgInfo.Rows; y++)
            {
                var row = png.ReadRow(y).Scanline;

                if (blendOp == BlendOps.APNGBlendOpSource && info.buffer.colorType.In(Texture.ColorType.RGB, Texture.ColorType.RGBA)) bgColors.AddRange(row);
                else
                {
                    var yI = y * png.ImgInfo.Cols * info.buffer.colorBand;
                    for (var xI = 0; xI < row.Length; xI += colorNb)
                    {
                        if (blendOp == BlendOps.APNGBlendOpOver)
                        {
                            var a = colorNb == 4 ? row[xI + 3] : 255;
                            if (a > 0)
                            {
                                bgColors.RemoveRange(yI + xI, 4);
                                var px = new ArraySegment<int>(row, xI, 3).Select(p => a * p + (1 - a) * p);
                                bgColors.InsertRange(yI + xI, px.Append(a));
                            }
                        }
                        else if (blendOp == BlendOps.APNGBlendOpSource)
                        {
                            if (info.buffer.colorType == Texture.ColorType.palette && PLTE != null)
                            {
                                if (PLTE.Count > row[xI] * 3 + 2) bgColors.AddRange(PLTE.GetRange(row[xI] * 3, 3));
                                else throw new Exception("The palette is too small");
                            }
                            else if (info.buffer.colorType == Texture.ColorType.grayscaleA) bgColors.AddRange(new[] { row[xI], row[xI], row[xI], row[xI + 1] });
                            else if (info.buffer.colorType == Texture.ColorType.grayscale) bgColors.AddRange(new[] { row[xI], row[xI], row[xI] });
                        }
                    }
                }
                if (info.buffer.colorType == Texture.ColorType.palette && png.ImgInfo.Cols > row.Length) bgColors.AddRange(new int[(png.ImgInfo.Cols - row.Length) * info.buffer.colorBand]);
            }

            Logging.Log("Done: " + info.id + "\n" + png.ImgInfo.Rows + " rows in " + sw.Elapsed + "\nBlend Op: " + blendOp);
            sw.Stop();
            png.End();

            info.dispose = frame.fcTLChunk != null ? frame.fcTLChunk.DisposeOp : DisposeOps.APNGDisposeOpNone;
            return bgColors;
        }
    }
}
