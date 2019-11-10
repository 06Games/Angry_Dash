using AngryDash.Language;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

public class RessourcePackLoader : MonoBehaviour
{
    public TextAsset IDs;
    public UnityEngine.UI.Text Status;

    void Start()
    {
        StartCoroutine(Load());
    }

    int reloadEach = 25;
    IEnumerator Load()
    {
        var path = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/";
        var ids = IDs.text.Split(new string[] { "\n" }, System.StringSplitOptions.None);
        Status.text = LangueAPI.Get("native", "loadingRessources.state", "[0]/[1]", 0, ids.Length - 1);

        float maxMem = 0;
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        for (int i = 0; i < ids.Length; i++)
        {
            if (!string.IsNullOrEmpty(ids[i]))
            {
                string baseID = ids[i].Replace("\r", "");

                FileFormat.JSON json = new FileFormat.JSON("");
                string jsonID = path + baseID + ".json";
                if (File.Exists(jsonID)) json = new FileFormat.JSON(File.ReadAllText(jsonID));
                AngryDash.Image.JSON_PARSE_DATA jsonData = AngryDash.Image.JSON_API.Parse(baseID, json);

                for (int f = 0; f < 4; f++)
                    AngryDash.Image.Sprite_API.Load(jsonData.path[f], jsonData.border[f]);

                yield return new WaitForEndOfFrame();
                Status.text = LangueAPI.Get("native", "loadingRessources.state", "[0]/[1]", i, ids.Length - 1);

                var mem = Profiler.GetTotalReservedMemoryLong() / 1048576f;
                if (maxMem < mem) maxMem = mem;
                if (i / (float)reloadEach == i / reloadEach) Resources.UnloadUnusedAssets();
            }
        }

        sw.Stop();
        Debug.Log(sw.Elapsed.TotalSeconds.ToString("0.000").Replace(".", ",") + "\n" + maxMem.ToString("0.000").Replace(".", ","));

        SceneManager.LoadScene("Home", SceneManager.args);
    }
}
