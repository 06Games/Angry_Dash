using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Manager = UnityEngine.SceneManagement.SceneManager;
using UnityEngine.UI;

public class SceneManager : MonoBehaviour
{
    GameObject loadingScreenObj;
    AsyncOperation async;

    public static bool CanChange { get; set; } = true;
    public static event System.Action<Scene> OnSceneChange;

    public static string[] args { get; private set; }

    static SceneManager GetLSC()
    {
        var LSC = FindObjectOfType<SceneManager>();
        if (LSC == null) throw new System.NullReferenceException("No Loading Screen");
        else return LSC;
    }

    public static void LoadScene(string Scene) { LoadScene(Scene, null, false); }
    public static void LoadScene(string Scene, bool keep = false) { LoadScene(Scene, null, keep); }
    public static void LoadScene(string Scene, string[] Args, bool keep = false)
    {
        var LSC = GetLSC();
        if (LSC.async != default) return;
        args = Args == null ? new string[0] : Args;

        LSC.loadingScreenObj = LSC.transform.GetChild(0).gameObject;

        string bgPath = Application.persistentDataPath + "/Ressources/default/textures/native/GUI/other/loadingScreen/splashScreens/";
#if UNITY_STANDALONE_WIN
        System.Collections.Generic.IEnumerable<CodeProject.FileData> files = CodeProject.FastDirectoryEnumerator.EnumerateFiles(bgPath, "* basic.png");
#else
        System.Collections.Generic.IEnumerable<System.IO.FileInfo> files = new System.IO.DirectoryInfo(bgPath).EnumerateFiles("* basic.png");
#endif

        if (files.Count() > 0)
        {
            int bgIndex = Random.Range(0, files.Count());
            string bgID = files.ElementAt(bgIndex).Path.Remove(0, (Application.persistentDataPath + "/Ressources/default/textures/").Length);
            Debug.Log(bgIndex + " - " + files.Count() + "\n" + bgID);
            LSC.loadingScreenObj.transform.GetChild(1).GetChild(0).GetComponent<AngryDash.Image.Reader.UImage_Reader>().SetID(bgID.Remove(bgID.Length - " basic.png".Length)).Load();
        }

        LSC.StartCoroutine(LSC.LoadingScreen(Scene, keep));
    }

    IEnumerator LoadingScreen(string scene, bool keep)
    {
        yield return new WaitUntil(() => CanChange);
        loadingScreenObj.SetActive(true);


        var temp = Manager.CreateScene("LoadingScene");
        var oldScene = Manager.GetActiveScene();
        if (!keep)
        {
            Manager.MoveGameObjectToScene(gameObject, temp);
            Manager.MoveGameObjectToScene(new GameObject().AddComponent<Camera>().gameObject, temp);
            Manager.SetActiveScene(temp);

            var oldName = oldScene.name;
            yield return Manager.UnloadSceneAsync(oldScene);
            Logging.Log($"Scene '{oldName}' unloaded");
        }

        if (Manager.GetSceneByName(scene) == default) async = Manager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        else
        {
            foreach (GameObject go in Manager.GetSceneByName(scene).GetRootGameObjects()) go.SetActive(true);
            Completed();
            yield break;
        }


        async.allowSceneActivation = false;
        async.completed += (t) => Completed();
        var slider = loadingScreenObj.transform.GetChild(0).GetComponent<Slider>();
        while (async != null && !async.isDone)
        {
            slider.value = async.progress;
            if (async.progress == 0.9f)
            {
                async.allowSceneActivation = true;
                slider.value = 1f;
            }
            yield return null;
        }

        void Completed()
        {
            if (keep)
            {
                loadingScreenObj.SetActive(false);
                foreach (GameObject go in oldScene.GetRootGameObjects()) go.SetActive(false);
            }
            async = default;

            var loadedScene = Tools.SceneManagerExtensions.GetScenesByName(scene).LastOrDefault();
            Manager.SetActiveScene(loadedScene);
            Logging.Log($"Scene '{loadedScene.name}' loaded");

            Manager.UnloadSceneAsync(temp);
            OnSceneChange?.Invoke(loadedScene);
        }
    }
}