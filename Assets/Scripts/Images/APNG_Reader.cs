using LibAPNG;
using System.Collections;
using System.IO;
using UnityEngine;

public class APNG_Reader : MonoBehaviour
{

    public string id = "";
    float FramesPerSeconds = 60;

    Sprite[] Frames;
    int Frame;

    void Start()
    {
        Sprite_API.Sprite_API_Data data = Sprite_API.Sprite_API.GetSprites(id);

        if (data == null)
        {
            Frames = new Sprite[] { GetComponent<UnityEngine.UI.Image>().sprite };
            FramesPerSeconds = 60F;
        }
        else
        {
            Frames = data.Frames;
            FramesPerSeconds = data.FramePerSeconds;
        }

        StartAnimating();
    }

    public void StartAnimating()
    {
        if (Frames.Length > 1)
        {
            Frame = 0;
            StartCoroutine(APNG());
        }
        else if (Frames.Length == 1)
            GetComponent<UnityEngine.UI.Image>().sprite = Frames[0];
    }
    public void StopAnimating()
    {
        if (Frames.Length > 1)
        {
            StopCoroutine(APNG());
            Frame = -1;
        }
        else if (Frames.Length == 1)
            GetComponent<UnityEngine.UI.Image>().sprite = Frames[0];
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
