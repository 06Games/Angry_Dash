using AngryDash.Image.Reader;
using System.IO;
using System.Linq;
using Tools;
using UnityEngine;
using UnityEngine.UI;

public class RessourcePackManager : MonoBehaviour
{
    public LoadingScreenControl LS;
    string[] RPs;
    int category = 0;

    string[] rpResult;

    public void ChangCategory(int cat) { category = cat; Refresh(); }

    void Start() { Refresh(); }
    public void Refresh()
    {
        for (int i = 0; i < transform.GetChild(0).childCount; i++)
            transform.GetChild(0).GetChild(i).GetComponent<Button>().interactable = i != category;

        if (category == 0) RPs = Directory.GetDirectories(Application.persistentDataPath + "/Ressources/");
        else if (category == 1)
        {
            if (InternetAPI.IsConnected())
            {
                string URL = "https://06games.ddns.net/Projects/Games/Angry%20Dash/ressources/?required=False&v=" + Application.version;
                System.Net.WebClient client = new System.Net.WebClient();
                System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                rpResult = client.DownloadString(URL).Split(System.StringSplitOptions.RemoveEmptyEntries, "<BR />");
            }
            else rpResult = new string[0];
            RPs = new string[rpResult.Length];
            for (int i = 0; i < rpResult.Length; i++)
                RPs[i] = rpResult[i].Remove(0, "<name>".Length).Split("</name>")[0].Replace("\n", "").Replace("\r", "");
        }

        Transform Content = transform.GetChild(1).GetChild(0).GetChild(0);
        GameObject Template = Content.GetChild(0).gameObject;

        for (int i = 1; i < Content.childCount; i++)
            Destroy(Content.GetChild(i).gameObject);

        for (int i = 0; i < RPs.Length; i++)
        {
            Transform go = Instantiate(Template, Content).transform;
            if (category == 1)
                go.GetComponent<RectTransform>().sizeDelta = new Vector2(go.GetComponent<RectTransform>().sizeDelta.x, 100);
            int param = i;
            go.GetComponent<Button>().onClick.AddListener(() => SetRP(param));
            if (category == 1)
            {
                go.GetChild(3).GetComponent<Button>().onClick.AddListener(() => Download(param));
                string RP = Application.persistentDataPath + "/Ressources/" + Path.GetFileNameWithoutExtension(RPs[i]);
                go.GetChild(3).gameObject.SetActive(!Directory.Exists(RP));

                if (Directory.Exists(RP))
                {
                    go.GetChild(4).GetComponent<Button>().onClick.AddListener(() => Download(param));
                    long dirSize = new DirectoryInfo(RP).GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
                    if (!long.TryParse(rpResult[i].Split("<size>")[1].Replace("\n", "").Replace("\r", "").TrimEnd("B</size>"), out long rpSize))
                        go.GetChild(4).gameObject.SetActive(false);
                    else go.GetChild(4).gameObject.SetActive(dirSize != rpSize);
                }
                else go.GetChild(4).gameObject.SetActive(false);
            }

            string name = "";
            if (category == 0) name = new DirectoryInfo(RPs[i]).Name;
            else if (category == 1) name = Path.GetFileNameWithoutExtension(RPs[i]);

            go.GetComponent<Button>().interactable = name != ConfigAPI.GetString("ressources.pack");
            go.GetChild(2).gameObject.SetActive(name == ConfigAPI.GetString("ressources.pack"));

            if (category == 0)
            {
                string[] data = new string[0];
                if (File.Exists(RPs[i] + "/info.ini")) data = File.ReadAllLines(RPs[i] + "/info.ini");

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
                        else if (infos[0] == "wallpaper") wallpaperPath = RPs[i] + "/" + infos[1];
                    }
                }
                go.GetChild(1).GetComponent<Text>().text = description;
                if (!string.IsNullOrEmpty(wallpaperPath)) go.GetComponent<UImage_Reader>().SetPath(wallpaperPath);
                else go.GetComponent<UImage_Reader>().SetID("native/GUI/settingsMenu/ressourcePacks/defaultBackground");
            }
            else go.GetComponent<UImage_Reader>().SetID("native/GUI/settingsMenu/ressourcePacks/officialBackground");

            go.name = name;
            go.GetChild(0).GetComponent<Text>().text = name;

            go.gameObject.SetActive(true);
        }
    }

    public void SetRP(int index)
    {
        if (category == 1)
        {
            string RP = Application.persistentDataPath + "/Ressources/" + Path.GetFileNameWithoutExtension(RPs[index]);
            if (!Directory.Exists(RP)) return;
        }

        if (category == 0) ConfigAPI.SetString("ressources.pack", new DirectoryInfo(RPs[index]).Name);
        else if (category == 1) ConfigAPI.SetString("ressources.pack", Path.GetFileNameWithoutExtension(RPs[index]));
        for (int i = 0; i < transform.GetChild(1).GetChild(0).GetChild(0).childCount - 1; i++)
        {
            Transform go = transform.GetChild(1).GetChild(0).GetChild(0).GetChild(i + 1);
            go.GetComponent<Button>().interactable = i != index;
            go.GetChild(2).gameObject.SetActive(i == index);
        }
    }

    public void Download(int index)
    {
        LS.LoadScreen("Start", new string[] { "Dependencies", "Home", rpResult[index] }, true);
        transform.GetChild(1).GetChild(0).GetChild(0).GetChild(index + 1).GetChild(3).gameObject.SetActive(false);
    }

    public void ApplyRP()
    {
        CacheManager.Dictionary.Static().dictionary.Remove("Ressources/textures/json");
        CacheManager.Dictionary.Static().dictionary.Remove("Ressources/textures");
        LS.LoadScreen("Load", new string[] { "Settings", "Ressource Pack" });
    }

    string Format(string s)
    {
        s = s.Replace("\\t", "\t");
        s = s.Replace("\\n", "\n");
        return s;
    }
}
