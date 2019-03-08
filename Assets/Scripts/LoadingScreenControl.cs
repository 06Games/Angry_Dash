using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenControl : MonoBehaviour {
    public GameObject loadingScreenObj;
    public Slider slider;
    public Sprite[] Backgrounds;
    AsyncOperation async;

    public static bool CanChange { get; set; } = true;
    public static event Tools.BetterEventHandler OnSceneChange;

    public void LoadScreen(string Scene, bool keep = false) { LoadScreen(Scene, null, keep); }

    public void LoadScreen(string Scene, string[] args, bool keep = false)
    {
        if (async != default) return;
        if (GameObject.Find("Temp_var") != null)
            Destroy(GameObject.Find("Temp_var"));
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

    public string[] GetArgs() {
        if (GameObject.Find("Temp_var") == null) return null;
        else {
            string[] args = GameObject.Find("Temp_var").GetComponent<Text>().text.Split(new string[] { "\\newParam" }, System.StringSplitOptions.None);
            Destroy(GameObject.Find("Temp_var"));
            return args;
        }
    }

    IEnumerator LoadingScreen(string scene, bool keep) {
        if (!CanChange) yield return new WaitUntil(() => CanChange);

        LoadSceneMode lsm = LoadSceneMode.Single;
        if (keep) lsm = LoadSceneMode.Additive;

        loadingScreenObj.SetActive(true);
        if (SceneManager.GetSceneByName(scene) == default) async = SceneManager.LoadSceneAsync(scene, lsm);
        else {
            async = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            foreach (GameObject go in SceneManager.GetSceneByName(scene).GetRootGameObjects()) go.SetActive(true);
        }

        async.completed += new System.Action<AsyncOperation>((task) => {
            if (keep)
            {
                async = default;
                loadingScreenObj.SetActive(false);
                Scene oldScene = SceneManager.GetActiveScene();
                foreach (GameObject go in oldScene.GetRootGameObjects()) go.SetActive(false);
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene));
            }
            if (OnSceneChange != null) OnSceneChange.Invoke(null, new Tools.BetterEventArgs(scene));
        });
        async.allowSceneActivation = false;
        while (async.isDone == false)
        {
            slider.value = async.progress;
            if (async.progress == 0.9f)
            {
                slider.value = 1f;
                async.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}