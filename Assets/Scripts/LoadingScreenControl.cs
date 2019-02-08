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

    public void LoadScreen(string Scene) { LoadScreen(Scene, null); }

    public void LoadScreen(string Scene, string[] args)
    {
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

        StartCoroutine(LoadingScreen(Scene));
    }

    public string[] GetArgs() {
        if (GameObject.Find("Temp_var") == null) return null;
        else {
            string[] args = GameObject.Find("Temp_var").GetComponent<Text>().text.Split(new string[] { "\\newParam" }, System.StringSplitOptions.None);
            Destroy(GameObject.Find("Temp_var"));
            return args;
        }
    }

    IEnumerator LoadingScreen(string Scene) {
        if (!CanChange) yield return new WaitUntil(() => CanChange);

        loadingScreenObj.SetActive(true);
        async = SceneManager.LoadSceneAsync(Scene);
        if (OnSceneChange != null) async.completed += new System.Action<AsyncOperation>((task) => OnSceneChange.Invoke(null, new Tools.BetterEventArgs(Scene)));
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