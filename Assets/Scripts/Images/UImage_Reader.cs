using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AngryDash.Image.Reader
{
    public class UImage_Reader : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public UImage_Reader SetID(string id) { baseID = id; return this; }

        //Configuration
        public string baseID;
        public bool TextConfiguration = true;

        //Parameters
        Sprite_API_Data[] data = new Sprite_API_Data[4];
        GameObject obj;

        //Data
        new Coroutine[] animation = new Coroutine[4];
        System.Diagnostics.Stopwatch[] animationTime = new System.Diagnostics.Stopwatch[4];
        uint[] Played = new uint[4];
        int[] Frame = new int[4];
        [HideInInspector]
        public JSON_PARSE_DATA.Type[] Type = new JSON_PARSE_DATA.Type[4];

        public Vector2 FrameSize
        {
            get
            {
                if (data[0] is null) return default;
                else if (data[0].Frames is null) return default;
                else return new Vector2(data[0].Frames[0].texture.width, data[0].Frames[0].texture.height);
            }
        }

        void Start() { if (!string.IsNullOrEmpty(baseID)) Load(); }
        public UImage_Reader Load(Sprite_API_Data spriteData) { data = new Sprite_API_Data[] { spriteData }; return this; }
        public UImage_Reader Load(Sprite_API_Data[] spriteData) { data = spriteData; return this; }
        public UImage_Reader Load()
        {
            JSON_PARSE_DATA jsonData = JSON_API.Parse(baseID);

            data = new Sprite_API_Data[4];
            for (int i = 0; i < data.Length; i++) data[i] = Sprite_API.GetSprites(jsonData.path[i], jsonData.border[i]);

            ApplyJson(jsonData);
            return this;
        }
        public UImage_Reader SetPath(string id)
        {
            JSON_PARSE_DATA jsonData = JSON_API.Parse(id, null, true);

            data = new Sprite_API_Data[4];
            for (int i = 0; i < data.Length; i++)
                data[i] = Sprite_API.GetSprites(jsonData.path[i], jsonData.border[i]);

            ApplyJson(jsonData);
            return this;
        }


        public UImage_Reader ApplyJson(JSON_PARSE_DATA data)
        {
            if (obj == null) obj = gameObject;
            //Textures
            if (GetComponent<Selectable>() != null)
            {
                lastInteractable = obj.GetComponent<Selectable>().interactable;
                obj.GetComponent<Selectable>().transition = Selectable.Transition.None;
            }
            Type = data.type;

            if (TextConfiguration)
            {
                //Text
                Text[] texts = GetComponentsInChildren<Text>(true);
                for (int i = 0; i < texts.Length; i++)
                {

                    texts[i].color = data.textColor; //Color
                    texts[i].fontStyle = data.textStyle; //Font Style
                    texts[i].alignment = data.textAnchor; //Font Alignment
                    texts[i].resizeTextForBestFit = data.textResize; //Font Resize
                    texts[i].fontSize = data.textSize;//Font Size
                    texts[i].resizeTextMinSize = data.textResizeMinAndMax[0]; //Font Resize Min
                    texts[i].resizeTextMaxSize = data.textResizeMinAndMax[1]; //Font Resize Max
                }
            }

            StartAnimating(0);
            return this;
        }

        void OnEnable() { StartAnimating(0); }
        public void OnPointerEnter(PointerEventData eventData) { if (lastInteractable) StartAnimating(1, 1); }
        public void OnPointerExit(PointerEventData eventData) { if (lastInteractable) StartAnimating(1, -1); }
        public void OnPointerDown(PointerEventData pointerEventData) { if (lastInteractable) StartAnimating(2, 1); }
        public void OnPointerUp(PointerEventData pointerEventData) { if (lastInteractable) StartAnimating(2, -1); }
        bool lastInteractable = true;
        public bool autoChange { get; set; } = true;
        void Update()
        {
            if (obj == null) obj = gameObject;
            if (obj.GetComponent<Selectable>() == null) return;
            else if (!autoChange) { lastInteractable = false; return; }

            if (obj.GetComponent<Selectable>().interactable != lastInteractable)
            {
                if (obj.GetComponent<Selectable>().interactable) StartAnimating(3, -1);
                else StartAnimating(3, 1);
                lastInteractable = obj.GetComponent<Selectable>().interactable;
            }
        }

        public void StartAnimating(int index) { StartAnimating(index, 1, false); }
        public void StartAnimating(int index, int frameAddition, bool keepFrame = true)
        {
            if (data == null) return;
            if (index >= data.Length) return;
            if (data[index] == null) return;

            if ((int)Type[index] <= 3 || (Type[index] == JSON_PARSE_DATA.Type.Fit & GetComponent<UnityEngine.UI.Image>() != null))
            {
                if (obj?.name == name + " - " + GetType().Name) Destroy(obj);
                obj = gameObject;
            }
            else if (obj == null || obj.name != name + " - " + GetType().Name)
            {
                obj = Instantiate(gameObject, transform);
                obj.name = name + " - " + GetType().Name;
                Destroy(obj.GetComponent<UImage_Reader>());
                obj.AddComponent<AspectRatioFitter>();
            }
            if (GetComponent<UnityEngine.UI.Image>() != null) GetComponent<UnityEngine.UI.Image>().enabled = (int)Type[index] <= 3;
            else if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().enabled = (int)Type[index] <= 3;

            SpriteDrawMode type = (int)Type[index] > 3 ? SpriteDrawMode.Simple : (SpriteDrawMode)Type[index];
            if (obj.GetComponent<UnityEngine.UI.Image>() != null) obj.GetComponent<UnityEngine.UI.Image>().type = (UnityEngine.UI.Image.Type)type;
            else if (obj.GetComponent<SpriteRenderer>() != null) obj.GetComponent<SpriteRenderer>().drawMode = type;
            if (Type[index] == JSON_PARSE_DATA.Type.Fit)
            {
                if (obj.GetComponent<UnityEngine.UI.Image>() != null) obj.GetComponent<UnityEngine.UI.Image>().preserveAspect = true;
                else if (obj.GetComponent<SpriteRenderer>() != null) obj.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            }
            else if (Type[index] == JSON_PARSE_DATA.Type.Envelope) obj.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            if (data[index].Frames.Length > 1)
            {
                if (!keepFrame) Frame[index] = 0;
                Played[index] = 0;
                if (animation[index] != null) StopCoroutine(animation[index]);

                animationTime[index] = new System.Diagnostics.Stopwatch();
                animationTime[index].Start();
                animation[index] = StartCoroutine(APNG(index, frameAddition));
            }
            else if (data[index].Frames.Length == 1 & frameAddition > 0)
            {
                if (obj.GetComponent<UnityEngine.UI.Image>() != null) obj.GetComponent<UnityEngine.UI.Image>().sprite = data[index].Frames[0];
                else if (obj.GetComponent<SpriteRenderer>() != null) obj.GetComponent<SpriteRenderer>().sprite = data[index].Frames[0];
            }
            else if (data[index].Frames.Length == 1 & frameAddition < 0)
                StartAnimating(0, 1);
        }
        public void StopAnimating(int index) { StopAnimating(index, false); }
        public void StopAnimating(int index, bool keepFrame)
        {
            if (data == null) return;
            if (index >= data.Length) return;
            if (data[index] == null) return;
            if (data[index].Frames.Length > 1)
            {
                animationTime[index].Stop();
                if (animation[index] != null) StopCoroutine(animation[index]);
                if (!keepFrame) Frame[index] = 0;
            }

            if (!keepFrame) StartAnimating(0);
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
                    if (obj.GetComponent<UnityEngine.UI.Image>() != null) obj.GetComponent<UnityEngine.UI.Image>().sprite = data[index].Frames[frameIndex];
                    else if (obj.GetComponent<SpriteRenderer>() != null) obj.GetComponent<SpriteRenderer>().sprite = data[index].Frames[frameIndex];

                    Frame[index] = frameIndex;
                    animationTime[index].Restart();
                }
                yield return new WaitForEndOfFrame();
                animation[index] = StartCoroutine(APNG(index, frameAddition));
            }
            else StopAnimating(index, true);
        }
    }
}
