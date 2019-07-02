using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using AngryDash.Language;

public class RessourcePackLoader : MonoBehaviour
{
    public TextAsset IDs;
    public UnityEngine.UI.Text Status;

    void Start()
    {
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        string path = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/";
        string[] ids = IDs.text.Split(new string[] { "\n" }, System.StringSplitOptions.None);
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
            }
        }

        LoadingScreenControl LSC = GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>();
        LSC.LoadScreen("Home", LSC.GetArgs());

        sw.Stop();
        Debug.Log(sw.Elapsed.ToString("g"));
    }
}
