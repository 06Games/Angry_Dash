using System;
using System.Collections;
using System.IO;
using System.Linq;
using AngryDash.Image.Reader;
using Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Manager = UnityEngine.SceneManagement.SceneManager;
using Random = UnityEngine.Random;

/// <summary>Scene loading manager</summary>
public class SceneManager : MonoBehaviour
{
    private GameObject loadingScreenObj;
    private AsyncOperation async;

    /// <summary>Can the scene be changed</summary>
    public static bool CanChange { get; set; } = true;
    /// <summary>When scene loaded</summary>
    public static event Action<Scene> OnSceneChange;
    /// <summary>Loading arguments</summary>
    public static string[] args { get; private set; } = new string[0];

    private static SceneManager GetLSC()
    {
        var LSC = FindObjectOfType<SceneManager>();
        if (LSC == null) throw new NullReferenceException("No Loading Screen");
        return LSC;
    }

    /// <summary>Reload the active scene</summary>
    /// <param name="keepArgs">Should keep the arguments</param>
    public static void ReloadScene(bool keepArgs = true) { LoadScene(Manager.GetActiveScene().name, keepArgs ? args : null); }
    /// <summary>Reload the active scene</summary>
    /// <param name="args">Loading arguments</param>
    public static void ReloadScene(string[] args) { LoadScene(Manager.GetActiveScene().name, args); }

    /// <summary>Load a scene</summary>
    /// <param name="Scene">Scene name</param>
    public static void LoadScene(string Scene) { LoadScene(Scene, null); }
    /// <summary>Load a scene</summary>
    /// <param name="Scene">Scene name</param>
    /// <param name="keep">Caching the active scene</param>
    public static void LoadScene(string Scene, bool keep = false) { LoadScene(Scene, null, keep); }
    /// <summary>Load a scene</summary>
    /// <param name="Scene">Scene name</param>
    /// <param name="Args">Loading arguments</param>
    /// <param name="keep">Caching the active scene</param>
    public static void LoadScene(string Scene, string[] Args, bool keep = false)
    {
        var LSC = GetLSC();
        if (LSC.async != default) return;
        args = Args == null ? new string[0] : Args;

        LSC.loadingScreenObj = LSC.transform.GetChild(0).gameObject;

        var bgPath = Application.persistentDataPath + "/Ressources/default/textures/native/GUI/other/loadingScreen/splashScreens/";
#if UNITY_STANDALONE_WIN
        System.Collections.Generic.IEnumerable<string> files = CodeProject.FastDirectoryEnumerator.EnumerateFiles(bgPath, "* basic.png").Select(f => f.Path);
#else
        var files = new DirectoryInfo(bgPath).EnumerateFiles("* basic.png").Select(f => f.FullName);
#endif

        if (files.Count() > 0)
        {
            var bgIndex = Random.Range(0, files.Count());
            var bgID = files.ElementAt(bgIndex).Remove(0, (Application.persistentDataPath + "/Ressources/default/textures/").Length).Replace("\\", "/");
            LSC.loadingScreenObj.transform.GetChild(1).GetChild(0).GetComponent<UImage_Reader>().SetID(bgID.Remove(bgID.Length - " basic.png".Length)).LoadAsync();
        }

        LSC.StartCoroutine(LSC.LoadingScreen(Scene, keep));
    }

    private IEnumerator LoadingScreen(string scene, bool keep)
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
            foreach (var go in Manager.GetSceneByName(scene).GetRootGameObjects()) go.SetActive(true);
            Completed();
            yield break;
        }


        async.allowSceneActivation = false;
        async.completed += t => Completed();
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
                foreach (var go in oldScene.GetRootGameObjects()) go.SetActive(false);
            }
            async = default;

            var loadedScene = SceneManagerExtensions.GetScenesByName(scene).LastOrDefault();
            Manager.SetActiveScene(loadedScene);
            Logging.Log($"Scene '{loadedScene.name}' loaded");

            Manager.UnloadSceneAsync(temp);
            OnSceneChange?.Invoke(loadedScene);
        }
    }
}