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
    AngryDash.Image.Sprite_API_Data[] sp;
    AngryDash.Image.JSON_PARSE_DATA[] jsonData;

    public ColorPicker CP;

    private void Start()
    {
        if (string.IsNullOrEmpty(Editor.file)) gameObject.SetActive(false);

        CP.onValueChanged.AddListener((Color bgColor) => Select(bgColor));
    }

    void OnEnable()
    {
        if (sp == null) Charg();
        Selected = Editor.level.background.id;
        CP.CurrentColor = Editor.level.background.color;
    }

    void Select(int i) { Selected = i; Select(CP.CurrentColor); }
    void Select(Color bgColor)
    {
        if (sp == null) return;
        Transform bgContent = transform.GetChild(0).GetComponent<ScrollRect>().content;
        for (int i = 1; i < bgContent.childCount; i++)
        {
            Transform trans = bgContent.GetChild(i);
            trans.GetChild(0).GetComponent<Image>().color = bgColor;
            trans.GetComponent<Button>().interactable = i - 1 != Selected;
        }
        if (Selected >= 0 & Selected < sp.Length)
        {
            if (sp[Selected].Frames.Length > 0)
                CP.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = sp[Selected].Frames[0];
        }

        Editor.level.background = new Level.Background { id = Selected, color = bgColor };

        ActualiseFond(Editor);
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
            List<AngryDash.Image.Sprite_API_Data> sprites = new List<AngryDash.Image.Sprite_API_Data>();
            List<AngryDash.Image.JSON_PARSE_DATA> jsons = new List<AngryDash.Image.JSON_PARSE_DATA>();
            foreach (var file in files.OrderBy(f => f.Name, new EnumerableExtensions.Comparer<string>(EnumerableExtensions.CompareNatural)))
            {
                string bgName = Path.GetFileNameWithoutExtension(file.Name);
                string baseID = "native/BACKGROUNDS/" + bgName.Remove(bgName.Length - 6);

                AngryDash.Image.JSON_PARSE_DATA jData = AngryDash.Image.JSON_API.Parse(baseID);
                jsons.Add(jData);
                sprites.Add(AngryDash.Image.Sprite_API.GetSprites(jData.path[0], jData.border[0]));
            }
            sp = sprites.ToArray();
            jsonData = jsons.ToArray();
        }

        Transform bgContent = transform.GetChild(0).GetComponent<ScrollRect>().content;
        for (int i = 1; i < bgContent.childCount; i++) Destroy(bgContent.GetChild(i).gameObject);
        for (int i = 0; i < sp.Length; i++)
        {
            Transform go = Instantiate(bgContent.GetChild(0).gameObject, bgContent).transform;
            int button = i;
            go.GetComponent<Button>().onClick.AddListener(() => Select(button));
            go.GetChild(0).GetComponent<UImage_Reader>().Load(sp[i]);
            go.gameObject.SetActive(true);
        }
    }

    public void ActualiseFond(Editeur Editor)
    {
        Transform go = GameObject.Find("BackgroundDiv").transform;
        int selected = Editor.level.background.id;
        for (int i = 0; i < go.childCount; i++)
        {
            go.GetChild(i).GetComponent<Image>().color = Editor.level.background.color;
            UImage_Reader reader = go.GetChild(i).GetComponent<UImage_Reader>().SetID("native/BACKGROUNDS/" + selected);
            if (sp != null) reader.Load(sp[selected]).ApplyJson(jsonData[selected]);
            else go.GetChild(i).GetComponent<UImage_Reader>().Load();
        }
        Vector2 size = default;
        if (sp != null) size = sp[selected].Frames[0].Size();
        else size = go.GetChild(0).GetComponent<Image>().sprite.Size();
        go.GetComponent<CanvasScaler>().referenceResolution = size;
        float match = 1;
        if (size.y < size.x) match = 0;
        go.GetComponent<CanvasScaler>().matchWidthOrHeight = match;
    }
}
