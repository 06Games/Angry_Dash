using LibAPNG;
using System.Collections;
using System.Drawing;
using System.IO;
using UnityEngine;

public class APNG_Reader : MonoBehaviour {
    
    public string apngPath = "";
    public float FramesPerSeconds = 60;

    public Sprite[] Frames;
    public int Frame;

    void Start()
    {
        APNG apng = new APNG(apngPath);
        Frames = new Sprite[apng.Frames.Length];

        if (apng.IsSimplePNG)
        {
            Frames = new Sprite[1];
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(File.ReadAllBytes(apngPath));
            Frames[0] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
        }
        else { 
            if (apng.DefaultImage.fcTLChunk.DelayNum == 0) FramesPerSeconds = 10000;
            else if (apng.DefaultImage.fcTLChunk.DelayDen == 0) FramesPerSeconds = 100F / apng.DefaultImage.fcTLChunk.DelayNum;
            else FramesPerSeconds = (float)apng.DefaultImage.fcTLChunk.DelayDen / apng.DefaultImage.fcTLChunk.DelayNum;
            if (FramesPerSeconds == 0) FramesPerSeconds = 60;

            int i = 0;
            foreach (Frame frame in apng.Frames)
            {
                Texture2D text = new Texture2D(1, 1);
                text.LoadImage(frame.GetStream().ToArray());

                Texture2D tex = CreateTransparent((int)apng.DefaultImage.fcTLChunk.Width, (int)apng.DefaultImage.fcTLChunk.Height);


                tex.SetPixels((int)frame.fcTLChunk.XOffset, tex.height - (int)frame.fcTLChunk.YOffset - text.height, text.width, text.height,
                    text.GetPixels(0, 0, text.width, text.height));
                tex.Apply();

                Frames[i] = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
                i++;
            }
        }

        StartAnimating();
    }
    Texture2D CreateTransparent(int width, int height)
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

    public void StartAnimating()
    {
        Frame = 0;
        StartCoroutine(APNG());
    }
    public void StopAnimating() {
        StopCoroutine(APNG());
        Frame = -1;
    }

    IEnumerator APNG()
    {
        if (Frame < Frames.Length - 1)
            Frame = Frame + 1;
        else if (Frame != -1)
            Frame = 0;

        float Speed = 1F / FramesPerSeconds;
        yield return new WaitForSeconds(Speed);
        GetComponent<UnityEngine.UI.Image>().sprite = Frames[Frame];
        StartCoroutine(APNG());
    }
}
