using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Sprite_API;
using Unity.Jobs;

public class RessourcePackLoader : MonoBehaviour
{
    public TextAsset IDs;
    public UnityEngine.UI.Text Status;
    Sprite_API_Job job;
    JobHandle handle;
    Unity.Collections.NativeArray<Data> nativeData;
    bool loadStarted = false;

    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    void Start()
    {
        CacheManager.Dictionary.Static();
        StartCoroutine(Load());
    }

    IEnumerator Load()
    {
        sw.Start();

        string path = Application.persistentDataPath + "/Ressources/" + ConfigAPI.GetString("ressources.pack") + "/textures/";
        string[] ids = IDs.text.Split(new string[] { "\n" }, System.StringSplitOptions.None);
        List<Data> data = new List<Data>();
        for (int i = 0; i < ids.Length; i++)
        {
            if (!string.IsNullOrEmpty(ids[i]))
            {
                string baseID = ids[i].Replace("\r", "");

                FileFormat.JSON json = new FileFormat.JSON("");
                string jsonID = path + baseID + ".json";
                if (File.Exists(jsonID)) json = new FileFormat.JSON(File.ReadAllText(jsonID));
                JSON_PARSE_DATA jsonData = JSON_API.Parse(baseID, json);

                for (int f = 0; f < 4; f++)
                {
                    System.IntPtr pathPtr = System.Runtime.InteropServices.Marshal.StringToBSTR(jsonData.path[f]);
                    data.Add(new Data() { path = pathPtr, border = jsonData.border[f], forcePNG = 0 });
                }

                //yield return new WaitForEndOfFrame();
                yield return null;
            }
        }


        nativeData = new Unity.Collections.NativeArray<Data>(data.ToArray(), Unity.Collections.Allocator.Persistent);
        job = new Sprite_API_Job() { data = nativeData };
        loadStarted = true;
        handle = job.Schedule(nativeData.Length, 1);
        JobHandle.ScheduleBatchedJobs();
        //handle.Complete();
    }

    private void Update()
    {
        if (loadStarted) Status.text = LangueAPI.Get("native", "loadingRessources.state", "[0]/[1]", job.state / 4, (job.data.Length - 1) / 4);
        else Status.text = LangueAPI.Get("native", "loadingRessources.state", "[0]/[1]", "-", "-");
        if (handle.IsCompleted & loadStarted)
        {
            nativeData.Dispose();

            LoadingScreenControl LSC = GameObject.Find("LoadingScreen").GetComponent<LoadingScreenControl>();
            LSC.LoadScreen("Home", LSC.GetArgs());

            sw.Stop();
            Debug.Log(sw.Elapsed.ToString("g"));
        }
    }

    private void OnDisable()
    {
        handle.Complete();
        nativeData.Dispose();
    }
}
