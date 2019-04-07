using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Tools;

public class Background : MonoBehaviour
{

    public int Selected;
    public Editeur Editor;
    string file;
    Sprite_API.Sprite_API_Data[] sp;
    Sprite_API.JSON_PARSE_DATA[] jsonData;

    public ColorPicker CP;

    private void Start()
    {
        if (string.IsNullOrEmpty(Editor.file)) gameObject.SetActive(false);
    }

    public void Charg()
    {
        if (sp == null)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
            IEnumerable<CodeProject.FileData> files = CodeProject.FastDirectoryEnumerator.EnumerateFiles(Application.persistentDataPath + "/Ressources/default/textures/native/BACKGROUNDS/", "* basic.png", SearchOption.TopDirectoryOnly);
#else
            IEnumerable<FileInfo> files = new DirectoryInfo(Application.persistentDataPath + "/Ressources/default/textures/native/BACKGROUNDS/").EnumerateFiles("* basic.png", SearchOption.TopDirectoryOnly);
#endif
            List<Sprite_API.Sprite_API_Data> sprites = new List<Sprite_API.Sprite_API_Data>();
            List<Sprite_API.JSON_PARSE_DATA> jsons = new List<Sprite_API.JSON_PARSE_DATA>();
            foreach (var file in files.OrderBy(f => f.Name, new EnumerableExtensions.Comparer<string>(EnumerableExtensions.CompareNatural)))
            {
                string bgName = Path.GetFileNameWithoutExtension(file.Name);
                string baseID = "native/BACKGROUNDS/" + bgName.Remove(bgName.Length - 6);

                Sprite_API.JSON_PARSE_DATA jData = Sprite_API.Sprite_API.Parse(baseID);
                jsons.Add(jData);
                sprites.Add(Sprite_API.Sprite_API.GetSprites(jData.path[0], jData.border[0]));
            }
            sp = sprites.ToArray();
            jsonData = jsons.ToArray();
        }
    }

    void Update()
    {
        if (file != Editor.file)
        {
            Selected = Editor.level.background.id;
            CP.CurrentColor = Editor.level.background.color;
            Page(0);
        }

        file = Editor.file;
        if (Editor.file != "")
        {
            Color32 bgColor = CP.CurrentColor;
            for (int i = 0; i < transform.GetChild(0).childCount - 2; i++)
            {
                Transform trans = transform.GetChild(0).GetChild(i + 1);
                trans.GetChild(0).GetComponent<Image>().color = bgColor;

                if (i + j == Selected) trans.GetComponent<Image>().color = new Color32(210, 210, 210, 255);
                else trans.GetComponent<Image>().color = new Color32(92, 92, 92, 255);
            }

            Editor.level.background = new Level.Background { id = Selected, color = bgColor };

            ActualiseFond(Editor);
        }
    }

    public void ChangFond(int i) { Selected = i + j; }

    public void ActualiseFond(Editeur Editor)
    {
        if (sp == null) Charg();

        Transform go = GameObject.Find("BackgroundDiv").transform;
        int selected = Editor.level.background.id;
        for (int i = 0; i < go.childCount; i++)
        {
            go.GetChild(i).GetComponent<Image>().color = Editor.level.background.color;
            go.GetChild(i).GetComponent<UImage_Reader>().Type[0] = jsonData[selected].type[0];
            go.GetChild(i).GetComponent<UImage_Reader>().Load(sp[selected], null);
        }
        Vector2 size = sp[selected].Frames[0].Size();
        go.GetComponent<CanvasScaler>().referenceResolution = size;
        float match = 1;
        if (size.y < size.x) match = 0;
        go.GetComponent<CanvasScaler>().matchWidthOrHeight = match;
    }

    int j = 0;
    public void Page(int p)
    {
        int bgNumber = 12;
        int bgDisplayed = 6;

        j = j + p;

        for (int i = 0; i < bgDisplayed; i++)
        {
            if (sp[Selected].Frames.Length > 0)
                transform.GetChild(0).GetChild(i + 1).GetChild(0).GetComponent<Image>().sprite = sp[j + i].Frames[0];
        }

        transform.GetChild(0).GetChild(0).GetComponent<Button>().interactable = j > 0;
        transform.GetChild(0).GetChild(7).GetComponent<Button>().interactable = (j + bgDisplayed) < bgNumber;
    }

    public void ChangeColorPickerBG(GameObject BG)
    {
        CP.transform.GetChild(0).GetChild(1).GetComponent<HexColorField>().displayAlpha = false; //Don't include alpha in hex codes
        CP.transform.GetChild(3).GetChild(3).gameObject.SetActive(false); //Sets to RGB
        BG.SetActive(true); //Actives preview
        if (sp[Selected].Frames.Length > 0) BG.GetComponent<Image>().sprite = sp[Selected].Frames[0]; //Sets the image to the preview
    }
}
