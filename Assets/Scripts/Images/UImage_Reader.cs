using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;

public class UImage_Reader : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{

    //Configuration
    public string baseID;

    //Parameters
    Sprite_API.Sprite_API_Data[] data = new Sprite_API.Sprite_API_Data[4];

    //Data
    new Coroutine[] animation = new Coroutine[4];
    System.Diagnostics.Stopwatch[] animationTime = new System.Diagnostics.Stopwatch[4];
    uint[] Played = new uint[4];
    int[] Frame = new int[4];
    public int[] Type = new int[4];

    void Start() { Load(); }
    public void Load(Sprite_API.Sprite_API_Data spriteData, string id = null)
    {
        data[0] = spriteData;
        baseID = id;

        if (id != null)
        {
            FileFormat.JSON.JSON json = new FileFormat.JSON.JSON("");
            if (File.Exists(Sprite_API.Sprite_API.spritesPath(baseID + ".json")))
                json = new FileFormat.JSON.JSON(File.ReadAllText(Sprite_API.Sprite_API.spritesPath(baseID + ".json")));
            LoadOthersComponents(json);
        }
        else StartAnimating(0);
    }
    public void Load(Sprite_API.Sprite_API_Data[] spriteData, string id = null)
    {
        data = spriteData;
        baseID = id;

        if (id != null)
        {
            FileFormat.JSON.JSON json = new FileFormat.JSON.JSON("");
            if (File.Exists(Sprite_API.Sprite_API.spritesPath(baseID + ".json")))
                json = new FileFormat.JSON.JSON(File.ReadAllText(Sprite_API.Sprite_API.spritesPath(baseID + ".json")));
            LoadOthersComponents(json);
        }
        else StartAnimating(0);
    }
    public void Load()
    {
        FileFormat.JSON.JSON json = new FileFormat.JSON.JSON("");
        if (File.Exists(Sprite_API.Sprite_API.spritesPath(baseID + ".json")))
            json = new FileFormat.JSON.JSON(File.ReadAllText(Sprite_API.Sprite_API.spritesPath(baseID + ".json")));

        //Textures
        FileFormat.JSON.Category category = json.GetCategory("textures");

        string[] paramNames = new string[] { "basic", "hover", "pressed", "disabled" };
        data = new Sprite_API.Sprite_API_Data[paramNames.Length];
        for (int i = 0; i < paramNames.Length; i++)
        {
            FileFormat.JSON.Category paramCategory = category.GetCategory(paramNames[i]);

            string path = Sprite_API.Sprite_API.spritesPath(baseID + " " + paramNames[i] + ".png");
            Vector4 border = new Vector4();
            if (paramCategory.ContainsValues)
            {
                //Border
                FileFormat.JSON.Category borderCategory = paramCategory.GetCategory("border");
                if (borderCategory.ContainsValues)
                {
                    if (borderCategory.ValueExist("left")) border.x = borderCategory.Value<float>("left");
                    if (borderCategory.ValueExist("right")) border.z = borderCategory.Value<float>("right");
                    if (borderCategory.ValueExist("top")) border.w = borderCategory.Value<float>("top");
                    if (borderCategory.ValueExist("bottom")) border.y = borderCategory.Value<float>("bottom");
                }

                //Path
                if (paramCategory.ValueExist("path"))
                    path = new FileInfo(Sprite_API.Sprite_API.spritesPath(baseID + ".json")).Directory.FullName +
                        "/" + paramCategory.Value<string>("path");

                if (paramCategory.ValueExist("type"))
                {
                    string ImageType = paramCategory.Value<string>("type");
                    if (ImageType == "Simple") Type[i] = 0;
                    else if (ImageType == "Sliced") Type[i] = 1;
                    else if (ImageType == "Tiled") Type[i] = 2;
                }
            }

            data[i] = Sprite_API.Sprite_API.GetSprites(path, border);
        }

        LoadOthersComponents(json);
    }


    void LoadOthersComponents(FileFormat.JSON.JSON json)
    {

        //Textures
        if (GetComponent<Selectable>() != null)
        {
            lastInteractable = GetComponent<Selectable>().interactable;
            GetComponent<Selectable>().transition = Selectable.Transition.None;
        }
        if (data == null)
        {
            data = new Sprite_API.Sprite_API_Data[4];
            data[0].Frames = new Sprite[] { GetComponent<Image>().sprite };
            data[0].Delay = new float[] { 60F };
            data[0].Repeat = 0;
        }
        StartAnimating(0);

        //Text
        Text[] texts = GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            FileFormat.JSON.Category category = json.GetCategory("text");
            //Color
            Color32 textColor = new Color32(255, 255, 255, 255);
            if (category.ValueExist("color")) HexColorField.HexToColor(category.Value<string>("color"), out textColor);
            texts[i].color = textColor;

            //Font Style
            if (category.ValueExist("fontStyle"))
            {
                string value = category.Value<string>("fontStyle");
                if (value == "Normal") texts[i].fontStyle = FontStyle.Normal;
                else if (value == "Bold") texts[i].fontStyle = FontStyle.Bold;
                else if (value == "Italic") texts[i].fontStyle = FontStyle.Italic;
                else if (value == "BoldAndItalic") texts[i].fontStyle = FontStyle.BoldAndItalic;
            }
            else texts[i].fontStyle = FontStyle.Normal;

            //Font Alignment
            FileFormat.JSON.Category fontAlignment = category.GetCategory("fontAlignment");
            if (fontAlignment.ContainsValues)
            {
                int horizontal = 0;
                int vertical = 0;

                if (fontAlignment.ValueExist("horizontal"))
                {
                    string horizontalValue = fontAlignment.Value<string>("horizontal");
                    if (horizontalValue == "Left") horizontal = 0;
                    else if (horizontalValue == "Center") horizontal = 1;
                    else if (horizontalValue == "Right") horizontal = 2;
                }
                else horizontal = (int)texts[i].alignment - ((int)texts[i].alignment / 3) * 3;

                if (fontAlignment.ValueExist("vertical"))
                {
                    string verticalValue = fontAlignment.Value<string>("vertical");
                    if (verticalValue == "Upper") vertical = 0;
                    else if (verticalValue == "Middle") vertical = 1;
                    else if (verticalValue == "Lower") vertical = 2;
                }
                else vertical = (int)texts[i].alignment / 3;

                texts[i].alignment = (TextAnchor)((vertical * 3) + horizontal);
            }
            else texts[i].alignment = TextAnchor.MiddleLeft;

            //Font Size
            FileFormat.JSON.Category fontSize = category.GetCategory("resize");
            if (fontSize.ValueExist("minSize") & fontSize.ValueExist("maxSize")) texts[i].resizeTextForBestFit = true;
            else texts[i].fontSize = 14;
            if (fontSize.ValueExist("minSize")) texts[i].resizeTextMinSize = fontSize.Value<int>("minSize");
            if (fontSize.ValueExist("maxSize")) texts[i].resizeTextMaxSize = fontSize.Value<int>("maxSize");
        }
    }

    public void OnPointerEnter(PointerEventData eventData) { if (lastInteractable) StartAnimating(1, 1); }
    public void OnPointerExit(PointerEventData eventData) { if (lastInteractable) StartAnimating(1, -1); }
    public void OnSelect(BaseEventData eventData) { if (lastInteractable) StartAnimating(2, 1); }
    public void OnDeselect(BaseEventData eventData) { if (lastInteractable) StartAnimating(2, -1); }
    bool lastInteractable = true;
    void Update()
    {
        if (GetComponent<Selectable>() == null) return;
        if (GetComponent<Selectable>().interactable != lastInteractable)
        {
            if (GetComponent<Selectable>().interactable) StartAnimating(3, -1);
            else StartAnimating(3, 1);
            lastInteractable = GetComponent<Selectable>().interactable;
        }
    }

    public void StartAnimating(int index) { StartAnimating(index, 1, false); }
    public void StartAnimating(int index, int frameAddition, bool keepFrame = true)
    {
        if (data[index] == null) return;
        
        if (GetComponent<Image>() != null) GetComponent<Image>().type = (Image.Type)Type[index];
        else if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().drawMode = (SpriteDrawMode)Type[index];

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
            if (GetComponent<Image>() != null) GetComponent<Image>().sprite = data[index].Frames[0];
            else if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().sprite = data[index].Frames[0];
        }
        else if (data[index].Frames.Length == 1 & frameAddition < 0)
            StartAnimating(0, 1);
    }
    public void StopAnimating(int index) { StopAnimating(index, false); }
    public void StopAnimating(int index, bool keepFrame)
    {
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
                if (GetComponent<Image>() != null) GetComponent<Image>().sprite = data[index].Frames[frameIndex];
                else if (GetComponent<SpriteRenderer>() != null) GetComponent<SpriteRenderer>().sprite = data[index].Frames[frameIndex];

                Frame[index] = frameIndex;
                animationTime[index].Restart();
            }
            yield return new WaitForEndOfFrame();
            animation[index] = StartCoroutine(APNG(index, frameAddition));
        }
        else StopAnimating(index, true);
    }
}
