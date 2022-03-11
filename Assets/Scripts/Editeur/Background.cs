using System.Collections.Generic;
using System.IO;
using System.Linq;
using AngryDash.Image;
using AngryDash.Image.JSON;
using AngryDash.Image.Reader;
using Tools;
using UnityEngine;
using UnityEngine.UI;
using Texture = AngryDash.Image.JSON.Texture;

public class Background : MonoBehaviour
{

    public int Selected;
    public Editeur Editor;
    private Sprite_API_Data[] sp;
    private Data[] jsonData;

    public ColorPicker CP;

    private void Start()
    {
        if (string.IsNullOrEmpty(Editor.file)) gameObject.SetActive(false);

        CP.onValueChanged.AddListener(bgColor => Select(bgColor));
    }

    private void OnEnable()
    {
        if (sp == null) Charg();
        Selected = Editor.level.background.id;
        CP.CurrentColor = Editor.level.background.color;
    }

    private void Select(int i) { Selected = i; Select(CP.CurrentColor); }

    private void Select(Color bgColor)
    {
        if (sp == null) return;
        Transform bgContent = transform.GetChild(0).GetComponent<ScrollRect>().content;
        for (var i = 1; i < bgContent.childCount; i++)
        {
            var trans = bgContent.GetChild(i);
            trans.GetChild(0).GetComponent<Image>().color = bgColor;
            trans.GetComponent<Button>().interactable = i - 1 != Selected;
        }
        if (Selected >= 0 & Selected < sp.Length)
        {
            if (sp[Selected].Frames.Count > 0)
            {
                var json = jsonData[Selected].DeepClone();
                json.textures[0].display = Texture.Display.Simple;
                CP.transform.GetChild(0).GetChild(0).GetComponent<UImage_Reader>().Load(sp[Selected]).ApplyJson(json);
            }
        }

        Editor.level.background = new Level.Background { id = Selected, color = bgColor };

        ActualiseFond(Editor);
    }

    public void Charg()
    {
        if (sp == null)
        {
#if UNITY_STANDALONE_WIN
            IEnumerable<CodeProject.FileData> files = CodeProject.FastDirectoryEnumerator.EnumerateFiles(Application.persistentDataPath + "/Ressources/default/textures/native/BACKGROUNDS/", "* basic.png", SearchOption.TopDirectoryOnly);
#else
            var files = new DirectoryInfo(Application.persistentDataPath + "/Ressources/default/textures/native/BACKGROUNDS/").EnumerateFiles("* basic.png", SearchOption.TopDirectoryOnly);
#endif
            var sprites = new List<Sprite_API_Data>();
            var jsons = new List<Data>();
            foreach (var file in files.OrderBy(f => f.Name, new EnumerableExtensions.Comparer<string>(EnumerableExtensions.CompareNatural)))
            {
                var bgName = Path.GetFileNameWithoutExtension(file.Name);
                var baseID = "native/BACKGROUNDS/" + bgName.Remove(bgName.Length - 6);

                var jData = JSON_API.Parse(baseID);
                jsons.Add(jData);
                sprites.Add(Sprite_API.GetSprites(jData.textures[0].path));
            }
            sp = sprites.ToArray();
            jsonData = jsons.ToArray();
        }

        Transform bgContent = transform.GetChild(0).GetComponent<ScrollRect>().content;
        for (var i = 1; i < bgContent.childCount; i++) Destroy(bgContent.GetChild(i).gameObject);
        for (var i = 0; i < sp.Length; i++)
        {
            var go = Instantiate(bgContent.GetChild(0).gameObject, bgContent).transform;
            var button = i;
            go.GetComponent<Button>().onClick.AddListener(() => Select(button));
            go.GetChild(0).GetComponent<UImage_Reader>().Load(sp[i]);
            go.gameObject.SetActive(true);
        }
    }

    public void ActualiseFond(Editeur Editor)
    {
        var go = GameObject.Find("BackgroundDiv").transform;
        var selected = Editor.level.background.id;
        for (var i = 0; i < go.childCount; i++)
        {
            go.GetChild(i).GetComponent<Image>().color = Editor.level.background.color;
            var reader = go.GetChild(i).GetComponent<UImage_Reader>().SetID("native/BACKGROUNDS/" + selected);
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
