using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UImage_Reader : MonoBehaviour
{
    //Configuration
    public string id = "";

    //Parameters
    Sprite_API.Sprite_API_Data data = new Sprite_API.Sprite_API_Data();

    //Data
    Image component;
    uint Played = 0;
    int Frame;


    void Start()
    {
        data = Sprite_API.Sprite_API.GetSprites(id);

        component = GetComponent<Image>();

        if (data == null)
        {
            data.Frames = new Sprite[] { component.GetComponent<Image>().sprite };
            data.Delay = new float[] { 60F };
            data.Repeat = 0;
        }

        StartAnimating();
    }

    public void StartAnimating()
    {
        if (data.Frames.Length > 1)
        {
            Frame = 0;
            Played = 0;
            StartCoroutine(APNG());
        }
        else if (data.Frames.Length == 1)
            component.sprite = data.Frames[0];
    }
    public void StopAnimating()
    {
        if (data.Frames.Length > 1)
        {
            StopCoroutine(APNG());
            Frame = -1;
        }
        else if (data.Frames.Length == 1)
            component.sprite = data.Frames[0];
    }

    IEnumerator APNG()
    {
        if (Frame < data.Frames.Length - 1)
            Frame = Frame + 1;
        else if (Frame != -1)
        {
            Frame = 0;
            Played++;
        }

        if (Played < data.Repeat | data.Repeat == 0)
        {
            float Speed = 1F / data.Delay[Frame];
            yield return new WaitForSeconds(Speed);
            component.sprite = data.Frames[Frame];
            StartCoroutine(APNG());
        }
        else StopAnimating();
    }
}
