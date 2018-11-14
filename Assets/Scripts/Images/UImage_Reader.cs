using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UImage_Reader : MonoBehaviour
{
    //Configuration
    public string id = "";
    public bool ASync = true;

    //Parameters
    Sprite[] Frames;
    float[] Delay = new float[] { 60 };
    uint MaxPlayed;

    //Data
    Image component;
    uint Played = 0;
    public int Frame;


    void Start()
    {
        Sprite_API.Sprite_API_Data data = null;
        /*if (ASync)
        {
            Task.Run(async () =>
            {
                Task<Sprite_API.Sprite_API_Data> ASyncMethode = StartASync();
                await Task.WhenAny(ASyncMethode);
                data = ASyncMethode.Result;
            });
        }
        else */data = Sprite_API.Sprite_API.GetSprites(id);

        component = GetComponent<Image>();

        if (data == null)
        {
            Frames = new Sprite[] { component.sprite };
            Delay = new float[] { 60F };
            MaxPlayed = 0;
        }
        else
        {
            Frames = data.Frames;
            Delay = data.Delay;
            MaxPlayed = data.Repeat;
        }

        StartAnimating();

    }

    async Task<Sprite_API.Sprite_API_Data> StartASync()
    {
        string path = Sprite_API.Sprite_API.spritesPath + id + ".png";
        return await Task.Run(() =>
        {
            return Sprite_API.Sprite_API.GetSprites(path, true);
        });
    }

    public void StartAnimating()
    {
        if (Frames.Length > 1)
        {
            Frame = 0;
            Played = 0;
            StartCoroutine(APNG());
        }
        else if (Frames.Length == 1)
            component.sprite = Frames[0];
    }
    public void StopAnimating()
    {
        if (Frames.Length > 1)
        {
            StopCoroutine(APNG());
            Frame = -1;
        }
        else if (Frames.Length == 1)
            component.sprite = Frames[0];
    }

    IEnumerator APNG()
    {
        if (Frame < Frames.Length - 1)
            Frame = Frame + 1;
        else if (Frame != -1)
        {
            Frame = 0;
            Played++;
        }

        if (Played < MaxPlayed | MaxPlayed == 0)
        {
            float Speed = 1F / Delay[Frame];
            yield return new WaitForSeconds(Speed);
            component.sprite = Frames[Frame];
            StartCoroutine(APNG());
        }
        else StopAnimating();
    }
}
