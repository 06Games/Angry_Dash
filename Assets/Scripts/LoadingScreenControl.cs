using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class LoadingScreenControl : MonoBehaviour
{
    public GameObject loadingScreenObj;
    public Slider slider;
    public Sprite[] Backgrounds;
    AsyncOperation async;

    public static bool CanChange { get; set; } = true;
    public static event System.Action<Scene> OnSceneChange;

    public static LoadingScreenControl GetLSC()
    {
        var LSC = FindObjectOfType<LoadingScreenControl>();
        if (LSC == null) throw new System.NullReferenceException("No Loading Screen");
        else return LSC;
    }

    public void LoadScreen(string Scene) { LoadScreen(Scene, null, false); }
    public void LoadScreen(string Scene, bool keep = false) { LoadScreen(Scene, null, keep); }
    public void LoadScreen(string Scene, string[] args, bool keep = false)
    {
        if (async != default) return;
        if (GameObject.Find("Temp_var") != null) Destroy(GameObject.Find("Temp_var"));
        if (args == null) args = new string[0];
        if (args.Length > 0)
        {
            GameObject var = new GameObject("Temp_var");
            DontDestroyOnLoad(var);
            Text txt = var.AddComponent<Text>();
            if (args.Length > 0) txt.text = args[0];
            for (int v = 1; v < args.Length; v++)
                txt.text = txt.text + "\\newParam" + args[v];
        }

        loadingScreenObj = transform.GetChild(0).gameObject;
        slider = loadingScreenObj.transform.GetChild(0).GetComponent<Slider>();

        System.Random rnd = new System.Random();
        int i = rnd.Next(0, Backgrounds.Length);
        if (i >= Backgrounds.Length)
            i = Backgrounds.Length - 1;
        loadingScreenObj.transform.GetChild(1).GetChild(0).GetComponent<Image>().sprite = Backgrounds[i];

        StartCoroutine(LoadingScreen(Scene, keep));
    }

    public string[] GetArgs()
    {
        if (GameObject.Find("Temp_var") == null) return null;
        else return GameObject.Find("Temp_var").GetComponent<Text>().text.Split(new string[] { "\\newParam" }, System.StringSplitOptions.None);
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