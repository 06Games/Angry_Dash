using AngryDash.Image.Reader;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RessourcePackManager : MonoBehaviour
{
    int category = 0;
    public RP[] RPs;
    [System.Serializable]
    public class RP
    {
        public string folderName;
        public string name;
        public string author;
        public string description;
        public long size;
        public string sha256;
        public bool optionnal;
        public string url;
        public string wallpaper;
    }

    public void ChangCategory(int cat) { category = cat; Refresh(); }
    public static string serverURL { get; private set; }

    void Awake() => serverURL = $"{_06Games.ServerAPI.apiUrl}angry-dash/ressources/?gameVersion={Application.version}";
    void Start() => Refresh();
    public void Refresh()
    {
        for (int i = 0; i < transform.GetChild(0).childCount; i++) transform.GetChild(0).GetChild(i).GetComponent<Button>().interactable = i != category;

        if (ConfigAPI.GetBool("ressources.disable"))
            RPs = new[] { new RP() { name = "Resource packs are disabled!", description = "Set <b>ressources.disable</b> to <b>False</b> to re-enable them" } };
        else if (category == 0)
        {
            RPs = Directory.GetDirectories(Application.persistentDataPath + "/Ressources/").Select(f =>
            {
                var dirInf = new DirectoryInfo(f);
                var rp = new RP()
                {
                    folderName = name = dirInf.Name,
                    size = dirInf.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length),
                    sha256 = Security.Hashing.SHA(Security.Hashing.Algorithm.SHA256, "")
                };
                if (!File.Exists(f + "/info.ini")) return rp;

                var ini = new FileFormat.INI.INI(f + "/info.ini").GetCategory("Configuration");
                rp.name = Format(ini.Value<string>("name") ?? rp.name);
                rp.author = Format(ini.Value<string>("author"));
                rp.description = Format(ini.Value<string>("description"));
                rp.wallpaper = f + "/" + Format(ini.Value<string>("wallpaper"));
                return rp;
            }).ToArray();
        }
        else if (category == 1 && InternetAPI.IsConnected())
        {
            System.Net.WebClient client = new System.Net.WebClient();
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            RPs = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, RP>>(client.DownloadString(serverURL)).Values.ToArray();
        }
        else RPs = new RP[0];

        Transform Content = transform.GetChild(1).GetChild(0).GetChild(0);
        for (int i = 1; i < Content.childCount; i++) Destroy(Content.GetChild(i).gameObject);
        GameObject Template = Content.GetChild(0).gameObject;


        for (int i = 0; i < RPs.Length; i++)
        {
            var rp = RPs[i];
            int param = i;
            Transform go = Instantiate(Template, Content).transform;

            //Could be selected/Is selected
            go.GetComponent<Button>().onClick.AddListener(() => SetRP(param));
            go.GetComponent<Button>().interactable = !ConfigAPI.GetBool("ressources.disable") && rp.folderName != ConfigAPI.GetString("ressources.pack");
            go.Find("Selected").gameObject.SetActive(rp.folderName == ConfigAPI.GetString("ressources.pack"));

            //Download btn
            string RP = Application.persistentDataPath + "/Ressources/" + rp.folderName;
            var downBtn = go.Find("Download");
            downBtn.GetComponent<Button>().onClick.AddListener(() => Download(param));
            downBtn.gameObject.SetActive(true);
            if (!Directory.Exists(RP)) downBtn.GetComponent<UImage_Reader>().baseID = "native/GUI/settingsMenu/ressourcePacks/download";
            else
            {
                var dirSize = new DirectoryInfo(RP).GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
                if (dirSize != rp.size && rp.url != null) downBtn.GetComponent<UImage_Reader>().baseID = "native/GUI/settingsMenu/ressourcePacks/update";
                else downBtn.gameObject.SetActive(false);
            }

            //Infos
            go.name = rp.name;
            go.Find("Title").GetComponent<Text>().text = rp.name;
            go.Find("Description").GetComponent<Text>().text = rp.description;
            go.Find("Author").GetComponent<Text>().text = string.IsNullOrWhiteSpace(rp.author) ? null : AngryDash.Language.LangueAPI.Get("native", "settings.ressourcesPack.author", "by [0]", rp.author);
            go.Find("Size").GetComponent<Text>().text = DependenciesManager.FileSizeUnit(rp.size);

            //Wallpaper
            if (!string.IsNullOrEmpty(rp.wallpaper) && System.Uri.TryCreate(rp.wallpaper, System.UriKind.Absolute, out var uri) && uri.Scheme != "file")
            {
                UnityThread.executeCoroutine(GetWall());
                IEnumerator GetWall()
                {
                    go.GetComponent<UImage_Reader>().Load();
                    var wall = UnityWebRequestTexture.GetTexture(rp.wallpaper);
                    yield return wall.SendWebRequest();
                    try
                    {
                        var tex = DownloadHandlerTexture.GetContent(wall);
                        go.GetComponent<UImage_Reader>().enabled = false;
                        go.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    }
                    catch {}
                }
            }
            else if (!string.IsNullOrEmpty(rp.wallpaper)) go.GetComponent<UImage_Reader>().SetPath(rp.wallpaper);

            go.gameObject.SetActive(true);
        }
    }

    public void SetRP(int index)
    {
        if (category == 1)
        {
            string RP = Application.persistentDataPath + "/Ressources/" + RPs[index].folderName;
            if (!Directory.Exists(RP)) return;
        }

        ConfigAPI.SetString("ressources.pack", RPs[index].folderName);
        for (int i = 0; i < transform.GetChild(1).GetChild(0).GetChild(0).childCount - 1; i++)
        {
            Transform go = transform.GetChild(1).GetChild(0).GetChild(0).GetChild(i + 1);
            go.GetComponent<Button>().interactable = i != index;
            go.Find("Selected").gameObject.SetActive(i == index);
        }
    }

    public void Download(int index)
    {
        SceneManager.LoadScene("Start", new string[] { "Dependencies", "Home", FileFormat.XML.Utils.ClassToXML(RPs[index]) }, true);
        transform.GetChild(1).GetChild(0).GetChild(0).GetChild(index + 1).Find("Download").gameObject.SetActive(false);
    }

    public void ApplyRP()
    {
        Cache.Dictionary.Remove("Ressources/textures/json");
        Cache.Dictionary.Remove("Ressources/textures");
        SceneManager.LoadScene("Load", new string[] { "Settings", "Ressource Pack" });
    }

    string Format(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        s = s.Replace("\\t", "\t");
        s = s.Replace("\\n", "\n");
        return s;
    }
}
