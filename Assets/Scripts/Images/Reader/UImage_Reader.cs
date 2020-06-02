using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AngryDash.Image.Reader
{
    [ExecuteInEditMode]
    public class UImage_Reader : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public UImage_Reader SetID(string id) { baseID = id; return this; }

        //Configuration
        public string baseID;
        public bool TextConfiguration = true;

        //Parameters
        List<Sprite_API_Data> data = new List<Sprite_API_Data>();
        UniversalImage Image;

        //Data
        Coroutine[] coroutines = new Coroutine[4];
        System.Diagnostics.Stopwatch[] animationTime = new System.Diagnostics.Stopwatch[4];
        readonly uint[] Played = new uint[4];
        readonly int[] Frame = new int[4];
        [HideInInspector]
        public JSON.Texture.Display[] Type = new JSON.Texture.Display[4];

        public Vector2 FrameSize
        {
            get
            {
                if (data is null) return default;
                if (data[0] is null) return default;
                else if (data[0].Frames is null) return default;
                else return new Vector2(data[0].Frames[0].texture.width, data[0].Frames[0].texture.height);
            }
        }

        void Start() { if (!string.IsNullOrEmpty(baseID)) LoadAsync(); }
        public UImage_Reader Load(Sprite_API_Data spriteData) { data = new List<Sprite_API_Data>() { spriteData }; return this; }
        public UImage_Reader Load(Sprite_API_Data[] spriteData) { data = new List<Sprite_API_Data>(spriteData); return this; }
        public UImage_Reader Load() { Load(JSON_API.Parse(baseID), false); return this; }
        public UImage_Reader LoadAsync() { Load(JSON_API.Parse(baseID), true); return this; }
        public UImage_Reader SetPath(string id) { Load(JSON_API.Parse(id, null, true), false); return this; }
        public UImage_Reader SetPathAync(string id) { Load(JSON_API.Parse(id, null, true), true); return this; }

        void Load(JSON.Data jsonData, bool async)
        {
            data = new List<Sprite_API_Data>(new Sprite_API_Data[System.Enum.GetNames(typeof(JSON.Texture.Type)).Length]);
            foreach (var texture in jsonData.textures) Sprite_API.LoadAsync(texture.path, texture.border, (s) => data[(int)texture.type] = s);
            if (async)
            {
#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying) StartCoroutine(lAsync());
                else
#endif
                    UnityThread.executeCoroutine(lAsync());
                System.Collections.IEnumerator lAsync()
                {
                    yield return new WaitUntil(() => data.Count(d => d != null) == jsonData.textures.Length | gameObject == null);
                    if (gameObject != null) ApplyJson(jsonData);
                }
            }
            else
            {
                while (data.Count(d => d != null) != jsonData.textures.Length);
                if (gameObject != null) ApplyJson(jsonData);
            }
        }


        public UImage_Reader ApplyJson(JSON.Data data)
        {
            if (Image == null) Image = new UniversalImage(gameObject);

            //Textures
            if (GetComponent<Selectable>() != null)
            {
                lastInteractable = GetComponent<Selectable>().interactable;
                GetComponent<Selectable>().transition = Selectable.Transition.None;
            }
            Type = data.textures.Select(t => t.display).ToArray();

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

            if (GetComponent<Selectable>()?.interactable == false) StartAnimating(3);
            else StartAnimating(0);
            return this;
        }

        void OnEnable() { if (autoChange) StartAnimating(0); }
        public void OnPointerEnter(PointerEventData eventData) { if (lastInteractable & autoChange) StartAnimating(1, 1); }
        public void OnPointerExit(PointerEventData eventData) { if (lastInteractable & autoChange) StartAnimating(1, -1); }
        public void OnPointerDown(PointerEventData pointerEventData) { if (lastInteractable & autoChange) StartAnimating(2, 1); }
        public void OnPointerUp(PointerEventData pointerEventData) { if (lastInteractable & autoChange) StartAnimating(2, -1); }
        bool lastInteractable = true;
        public bool autoChange { get; set; } = true;
        void Update()
        {
            if (Image == null) Image = new UniversalImage(gameObject);
            if (GetComponent<Selectable>() == null) return;
            else if (!autoChange) return;

            if (GetComponent<Selectable>().interactable != lastInteractable)
            {
                if (GetComponent<Selectable>().interactable) StartAnimating(3, -1);
                else StartAnimating(3, 1);
                lastInteractable = GetComponent<Selectable>().interactable;
            }
        }

        internal System.Action<int> animationChanged;
        public bool StartAnimating(int index) { return StartAnimating(index, 1, false); }
        public bool StartAnimating(int index, int frameAddition, bool keepFrame = true)
        {
            if (data == null) return false;
            if (index >= data.Count) return false;
            if (data[index] == null) return false;
            animationChanged?.Invoke(index);

            var thisImage = new UniversalImage(gameObject);
            if ((int)Type[index] <= 3 || (Type[index] == JSON.Texture.Display.Fit & thisImage.image != null))
            {
                if (Image?.gameObject?.name == name + " - " + GetType().Name)
                {
                    Destroy(Image.gameObject);
                    thisImage.enabled = (int)Type[index] <= 3;
                }
                Image = new UniversalImage(gameObject);
            }
            else if (Image?.gameObject == null || Image?.gameObject?.name != name + " - " + GetType().Name)
            {
                Image = new UniversalImage(Instantiate(gameObject, transform));
                Image.gameObject.name = name + " - " + GetType().Name;
                Destroy(Image.gameObject.GetComponent<UImage_Reader>());
                Image.gameObject.AddComponent<AspectRatioFitter>();
                thisImage.enabled = (int)Type[index] <= 3;
            }

            Image.type = (int)Type[index] > 3 ? SpriteDrawMode.Simple : (SpriteDrawMode)Type[index];
            if (Type[index] == JSON.Texture.Display.Fit)
            {
                if (Image.image != null) Image.image.preserveAspect = true;
                else if (Image.spriteR != null) gameObject.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            }
            else if (Type[index] == JSON.Texture.Display.Envelope) Image.gameObject.GetComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            if (data[index].Frames.Count > 1)
            {
                if (!keepFrame) Frame[index] = 0;
                Played[index] = 0;
                if (coroutines[index] != null) StopCoroutine(coroutines[index]);

                animationTime[index] = new System.Diagnostics.Stopwatch();
                animationTime[index].Start();
                coroutines[index] = StartCoroutine(APNG(index, frameAddition, keepFrame));
            }
            else if (data[index].Frames.Count == 1 & frameAddition > 0)
            {
                Image.sprite = data[index].Frames[0];
                return false;
            }
            else if (data[index].Frames.Count == 1 & frameAddition < 0) StartAnimating(0, 1);

            return true;
        }
        public void StopAnimating(int index) { StopAnimating(index, false); }
        public void StopAnimating(int index, bool keepFrame)
        {
            if (data == null) return;
            if (index >= data.Count) return;
            if (data[index] == null) return;
            if (data[index].Frames.Count > 1)
            {
                animationTime[index].Stop();
                if (coroutines[index] != null) StopCoroutine(coroutines[index]);
                if (!keepFrame) Frame[index] = 0;
            }

            if (!keepFrame) StartAnimating(0);
        }

        public int animationIndex { get; private set; }
        System.Collections.IEnumerator APNG(int index, int frameAddition, bool keepFrame)
        {
            animationIndex = index;
            int futurFrame = Frame[index] + frameAddition;
            if (futurFrame <= -1 | futurFrame >= data[index].Frames.Count)
            {
                Played[index]++;
                if (Played[index] < data[index].Repeat | data[index].Repeat == 0) Frame[index] = 0;
            }

            if (Played[index] < data[index].Repeat | data[index].Repeat == 0)
            {
                int frameIndex = -1;
                System.TimeSpan frameTime = new System.TimeSpan(0);
                bool stop = false;
                for (int i = futurFrame; i < data[index].Frames.Count & i > -1 & !stop; i = i + frameAddition)
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
                    Image.sprite = data[index].Frames[frameIndex];
                    Frame[index] = frameIndex;
                    animationTime[index].Restart();
                }

#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying) yield return null;
                else
#endif
                    yield return new WaitForEndOfFrame();
                coroutines[index] = StartCoroutine(APNG(index, frameAddition, keepFrame));
            }
            else StopAnimating(index, keepFrame);
        }
    }
}
