using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using AngryDash.Language;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

public class RessourcePackLoader : MonoBehaviour
{
    public TextAsset IDs;
    public UnityEngine.UI.Text Status;

    void Start()
    {
        if(path == null) path = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/";
        if (ids == null) ids = IDs.text.Split(new string[] { "\n" }, System.StringSplitOptions.None);
        Status.text = LangueAPI.Get("native", "loadingRessources.state", "[0]/[1]", index, ids.Length - 1);


        if (sw == null)
        {
            sw = new System.Diagnostics.Stopwatch();
            sw.Start();
        }

        StartCoroutine(Load());
    }

    static string path;
    static string[] ids;

    static int index = 0;
    int reloadEach = 25;


    static System.Diagnostics.Stopwatch sw;
    static float maxMem = 0;
    IEnumerator Load()
    {

        for (int i = index; i < ids.Length; i++)
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
                if (i / (float)reloadEach == i / reloadEach)
                {
                    index = i + 1;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                }
            }
        }

        LoadingScreenControl LSC = GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>();
        LSC.LoadScreen("Home", LSC.GetArgs());

        sw.Stop();
        Debug.Log(sw.Elapsed.TotalSeconds.ToString("0.000").Replace(".", ",") + "\n" + maxMem.ToString("0.000").Replace(".", ","));
    }
}
