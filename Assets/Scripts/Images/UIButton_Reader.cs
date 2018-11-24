using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButton_Reader : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{

    //Configuration
    public string[] id = new string[4];

    //Parameters
    Sprite_API.Sprite_API_Data[] data = new Sprite_API.Sprite_API_Data[4];

    //Data
    new Coroutine[] animation = new Coroutine[4];
    System.Diagnostics.Stopwatch[] animationTime = new System.Diagnostics.Stopwatch[4];
    Button component;
    uint[] Played = new uint[4];
    int[] Frame = new int[4];


    void Start()
    {
        for(int i = 0; i < 4; i++)
            data[i] = Sprite_API.Sprite_API.GetSprites(id[i]);

        component = GetComponent<Button>();

        if (data == null)
        {
            data = new Sprite_API.Sprite_API_Data[4];
            data[0].Frames = new Sprite[] { component.GetComponent<Image>().sprite };
            data[0].Delay = new float[] { 60F };
            data[0].Repeat = 0;
        }

        StartAnimating(0);
    }

    public void OnPointerEnter(PointerEventData eventData) { StartAnimating(1, 1); }
    public void OnPointerExit(PointerEventData eventData) { StartAnimating(1, -1); }
    public void OnSelect(BaseEventData eventData) { StartAnimating(3, 1); }
    public void OnDeselect(BaseEventData eventData) { StartAnimating(3, -1); }

    public void StartAnimating(int index) { StartAnimating(index, 1, false); }
    public void StartAnimating(int index, int frameAddition, bool keepFrame = true)
    {
        if (data[index].Frames.Length > 1)
        {
            if(!keepFrame) Frame[index] = 0;
            Played[index] = 0;
            if (animation[index] != null) StopCoroutine(animation[index]);

            animationTime[index] = new System.Diagnostics.Stopwatch();
            animationTime[index].Start();
            animation[index] = StartCoroutine(APNG(index, frameAddition));
        }
        else if (data[index].Frames.Length == 1)
            component.GetComponent<Image>().sprite = data[index].Frames[0];
    }
    public void StopAnimating(int index) { StopAnimating(index, false);  }
    public void StopAnimating(int index, bool keepFrame)
    {
        if (data[index].Frames.Length > 1)
        {
            animationTime[index].Stop();
            if (animation[index] != null) StopCoroutine(animation[index]);
            if(!keepFrame) Frame[index] = 0;
        }
        else if (data[index].Frames.Length == 1)
            component.GetComponent<Image>().sprite = data[index].Frames[0];
    }

    IEnumerator APNG(int index, int frameAddition)
    {
        int futurFrame = Frame[index] + frameAddition;
        if (futurFrame <= -1 | futurFrame >= data[index].Frames.Length)
        {
            Played[index]++;
            if (Played[index] < data[index].Repeat | data[index].Repeat == 0) Frame[index] = 0;
        }

        if (Played[index] < data[index].Repeat | data[index].Repeat == 0)
        {
            int frameIndex = -1;
            System.TimeSpan frameTime = new System.TimeSpan(0);
            bool stop = false;
            for (int i = futurFrame; i < data[index].Frames.Length & i > -1 & !stop; i = i + frameAddition)
            {
                int adding = 0;
                if (frameAddition < 0) adding = 1;

                System.TimeSpan delay = System.TimeSpan.FromSeconds(data[index].Delay[i + adding]);
                if ((animationTime[index].Elapsed - frameTime).TotalMilliseconds >= delay.TotalMilliseconds)
                {
                    frameTime = frameTime + delay;
                    frameIndex = i;
                }
                else stop = true;
            }

            if (frameIndex > -1)
            {
                component.GetComponent<Image>().sprite = data[index].Frames[frameIndex];
                Frame[index] = frameIndex;
                animationTime[index].Restart();
            }
            yield return new WaitForEndOfFrame();
            animation[index] = StartCoroutine(APNG(index, frameAddition));
        }
        else StopAnimating(index, true);
    }
}
