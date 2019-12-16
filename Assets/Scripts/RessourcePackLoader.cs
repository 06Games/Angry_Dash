using AngryDash.Language;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

public class RessourcePackLoader : MonoBehaviour
{
    public TextAsset IDs;
    public UnityEngine.UI.Text Status;
    public UnityEngine.UI.Text Infos;

    void Start()
    {
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        var path = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/";
        var ids = IDs.text.Split(new string[] { "\n" }, System.StringSplitOptions.None);
        Status.text = LangueAPI.Get("native", "loadingRessources.state", "[0]/[1]", 0, ids.Length - 1);

        float maxMem = 0;
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        double state = 0;
        UnityThread.executeCoroutine(UpdateData());
        for (int i = 0; i < ids.Length; i++)
        {
            if (!string.IsNullOrEmpty(ids[i]))
            {
                FileFormat.JSON json = new FileFormat.JSON("");
                string baseID = ids[i].Replace("\r", "");
                string jsonID = path + baseID + ".json";
                if (File.Exists(jsonID)) json = new FileFormat.JSON(File.ReadAllText(jsonID));
                AngryDash.Image.JSON.Data jsonData = AngryDash.Image.JSON_API.Parse(baseID, json);

                foreach (var texture in jsonData.textures)
                {
                    AngryDash.Image.Sprite_API.LoadAsync(texture.path, texture.border, (SAD) => state += 1D / jsonData.textures.Length);
                }
                yield return new WaitWhile(() => state < i);
            }
            else state++;
        }

        while (state < ids.Length) { yield return new WaitForEndOfFrame(); }

        sw.Stop();
        Debug.Log(sw.Elapsed.TotalSeconds.ToString("0.000").Replace(".", ",") + "\n" + maxMem.ToString("0.000").Replace(".", ","));

        SceneManager.LoadScene("Home", SceneManager.args);

        IEnumerator UpdateData()
        {
            while (state < ids.Length)
            {
                var mem = Profiler.GetTotalReservedMemoryLong() / 1048576f;
                if (maxMem < mem) maxMem = mem;

                var status = LangueAPI.Get("native", "loadingRessources.state", "[0]/[1]", state.ToString("0.00"), ids.Length.ToString());
                if (Status.text != status) Resources.UnloadUnusedAssets();

                Status.text = status;
                if (Infos.IsDestroyed()) break;
                Infos.text = LangueAPI.Get("native", "", "[0] GB - [1] elapsed", (Profiler.GetTotalReservedMemoryLong() / 1048576f / 1000F).ToString("0.000"), sw.Elapsed.ToString("mm\\:ss"));
                yield return new WaitForEndOfFrame();
            }
        }
    }
}
