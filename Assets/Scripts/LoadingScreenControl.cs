using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class LoadingScreenControl : MonoBehaviour
{
    GameObject loadingScreenObj;
    AsyncOperation async;

    public static bool CanChange { get; set; } = true;
    public static event System.Action<Scene> OnSceneChange;

    public static string[] args { get; private set; }
    public static LoadingScreenControl GetLSC()
    {
        var LSC = FindObjectOfType<LoadingScreenControl>();
        if (LSC == null) throw new System.NullReferenceException("No Loading Screen");
        else return LSC;
    }

    public void LoadScreen(string Scene) { LoadScreen(Scene, null, false); }
    public void LoadScreen(string Scene, bool keep = false) { LoadScreen(Scene, null, keep); }
    public void LoadScreen(string Scene, string[] Args, bool keep = false)
    {
        if (async != default) return;
        args = Args == null? new string[0]: Args;

        loadingScreenObj = transform.GetChild(0).gameObject;

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
            loadingScreenObj.transform.GetChild(1).GetChild(0).GetComponent<AngryDash.Image.Reader.UImage_Reader>().SetID(bgID.Remove(bgID.Length - " basic.png".Length)).Load();
        }

        StartCoroutine(LoadingScreen(Scene, keep));
    }

    IEnumerator LoadingScreen(string scene, bool keep)
    {
        yield return new WaitUntil(() => CanChange);
        loadingScreenObj.SetActive(true);


        var temp = SceneManager.CreateScene("LoadingScene");
        var oldScene = SceneManager.GetActiveScene();
        if (!keep)
        {
            SceneManager.MoveGameObjectToScene(gameObject, temp);
            SceneManager.MoveGameObjectToScene(new GameObject().AddComponent<Camera>().gameObject, temp);
            SceneManager.SetActiveScene(temp);

            var oldName = oldScene.name;
            yield return SceneManager.UnloadSceneAsync(oldScene);
            Logging.Log($"Scene '{oldName}' unloaded");
        }

        if (SceneManager.GetSceneByName(scene) == default) async = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        else
        {
            foreach (GameObject go in SceneManager.GetSceneByName(scene).GetRootGameObjects()) go.SetActive(true);
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
            SceneManager.SetActiveScene(loadedScene);
            Logging.Log($"Scene '{loadedScene.name}' loaded");

            SceneManager.UnloadSceneAsync(temp);
            OnSceneChange?.Invoke(loadedScene);
        }
    }
}