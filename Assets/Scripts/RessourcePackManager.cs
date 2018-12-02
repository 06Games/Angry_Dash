using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class RessourcePackManager : MonoBehaviour
{

    public LoadingScreenControl LS;
    string[] RPs;


    void Start() { Refresh(); }
    public void Refresh()
    {
        RPs = Directory.GetDirectories(Application.persistentDataPath + "/Ressources/");

        Transform Content = transform.GetChild(0).GetChild(0).GetChild(0);
        GameObject Template = Content.GetChild(0).gameObject;

        for (int i = 1; i < Content.childCount; i++)
            Destroy(Content.GetChild(i).gameObject);

        for (int i = 0; i < RPs.Length; i++)
        {
            string[] data = new string[0];
            if (File.Exists(RPs[i] + "/info.ini"))
                data = File.ReadAllLines(RPs[i] + "/info.ini");

            Transform go = Instantiate(Template, Content).transform;
            int param = i;
            go.GetComponent<Button>().onClick.AddListener(() => SetRP(param));

            string name = new DirectoryInfo(RPs[i]).Name;
            string description = "";
            string wallpaperPath = null;

            for (int l = 0; l < data.Length; l++)
            {
                string[] infos = data[l].Split(new string[] { " = " }, System.StringSplitOptions.None);
                infos[1] = Format(infos[1]);

                if (infos.Length > 1)
                {
                    if (infos[0] == "name") name = infos[1];
                    else if (infos[0] == "description") description = infos[1];
                    else if (infos[0] == "wallpaper") wallpaperPath = infos[1];
                }
            }

            go.name = name;
            go.GetChild(0).GetComponent<Text>().text = name;
            go.GetChild(1).GetComponent<Text>().text = description;
            if (!string.IsNullOrEmpty(wallpaperPath))
            {
                if (File.Exists(RPs[i] + "/" + wallpaperPath))
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(File.ReadAllBytes(RPs[i] + "/" + wallpaperPath));

                    Vector4 border = new Vector4();
                    go.GetComponent<Image>().sprite = Sprite.Create(tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(.5f, .5f),
                        100.0f, 0, SpriteMeshType.FullRect,
                        border);
                }
            }

            go.gameObject.SetActive(true);
        }
    }

    public void SetRP(int index)
    {
        ConfigAPI.SetString("ressources.pack", new DirectoryInfo(RPs[index]).Name);
    }
    public void ApplyRP()
    {
        LS.LoadScreen(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, new string[] { "Settings", "Ressource Pack" });
    }

    string Format(string s)
    {
        s = s.Replace("\\t", "\t");
        s = s.Replace("\\n", "\n");
        return s;
    }
}
