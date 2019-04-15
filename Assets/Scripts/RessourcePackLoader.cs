using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

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
        string path = Application.persistentDataPath + "/Ressources/default/textures/";
        string[] ids = IDs.text.Split(new string[] { "\n" }, System.StringSplitOptions.None);
        for (int i = 0; i < ids.Length; i++)
        {
            if (!string.IsNullOrEmpty(ids[i]))
            {
                string baseID = ids[i].Replace("\r", "");

                FileFormat.JSON json = new FileFormat.JSON("");
                string jsonID = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/" + baseID + ".json";
                if (File.Exists(jsonID)) json = new FileFormat.JSON(File.ReadAllText(jsonID));
                Sprite_API.JSON_PARSE_DATA jsonData = Sprite_API.Sprite_API.Parse(baseID, json);

                for (int f = 0; f < 4; f++)
                    Sprite_API.Sprite_API.Load(jsonData.path[f], jsonData.border[f]);

                yield return new WaitForEndOfFrame();
                Status.text = i + "/" + (ids.Length - 1);
            }
        }

        LoadingScreenControl LSC = GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>();
        LSC.LoadScreen("Home", LSC.GetArgs());
    }
}
